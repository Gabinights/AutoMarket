using AutoMarket.Constants;
using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using AutoMarket.Models.ViewModels;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailAuthService _emailAuthService;
        private readonly ILogger<ContaController> _logger;

        public ContaController(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            ApplicationDbContext context,
            IEmailAuthService emailAuthService,
            ILogger<ContaController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _emailAuthService = emailAuthService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validação Backend: Empresa DEVE ter NIF (reforço da validação do modelo)
            if (model.TipoConta == TipoConta.Empresa && string.IsNullOrWhiteSpace(model.NIF))
            {
                ModelState.AddModelError("NIF", "O NIF é obrigatório para contas empresariais.");
                return View(model);
            }

            // 1. Iniciar Transação
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Verificar unicidade do NIF DENTRO da transação (evita race condition)
                if (!string.IsNullOrEmpty(model.NIF))
                {
                    bool nifExists = await _context.Users.AnyAsync(u => u.NIF == model.NIF);
                    if (nifExists)
                    {
                        ModelState.AddModelError("NIF", "Este NIF já está associado a outra conta.");
                        await transaction.RollbackAsync();
                        return View(model);
                    }
                }

                var user = new Utilizador
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nome = model.Nome,
                    Morada = model.Morada,
                    PhoneNumber = model.Contacto,
                    NIF = model.NIF,
                    DataRegisto = DateTime.UtcNow
                };

                // 3. Criar o User (Identity)
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 4. Criar o Perfil Específico (Vendedor ou Comprador) baseado no enum
                    if (model.TipoConta == TipoConta.Vendedor || model.TipoConta == TipoConta.Empresa)
                    {
                        var vendedor = new Vendedor
                        {
                            UserId = user.Id,
                            TipoConta = model.TipoConta,
                            Status = StatusAprovacao.Pendente
                        };
                        _context.Vendedores.Add(vendedor);
                        await _userManager.AddToRoleAsync(user, Roles.Vendedor);
                    }
                    else
                    {
                        var comprador = new Comprador
                        {
                            UserId = user.Id,
                            ReceberNotificacoes = false
                        };
                        _context.Compradores.Add(comprador);
                        await _userManager.AddToRoleAsync(user, Roles.Comprador);
                    }

                    // 5. Gravar Perfil na BD
                    await _context.SaveChangesAsync();
                    
                    // 6. Confirmar a transação
                    await transaction.CommitAsync();

                    // Lógica de envio de email de confirmação
                    try
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action(nameof(ConfirmarEmail), "Conta",
                            new { userId = user.Id, token }, Request.Scheme);

                        if (string.IsNullOrEmpty(confirmationLink))
                        {
                            _logger.LogError("Falha ao gerar link de confirmação para {Email}", user.Email);
                        }
                        else
                        {
                            await _emailAuthService.EnviarEmailConfirmacaoAsync(user, confirmationLink);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao enviar email de confirmação para {Email}", user.Email);
                        // Registo completa mesmo se email falhar
                    }

                    return RedirectToAction("Index", "Home");
                }

                // Se o Identity falhar, fazer rollback
                await transaction.RollbackAsync();

                // Adicionar erros ao ModelState
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                    {
                        // MENSAGEM GENÉRICA: O atacante não sabe se o email existe ou se falhou outra coisa
                        ModelState.AddModelError(string.Empty, "Ocorreu um erro ao processar o registo. Por favor, verifique os dados ou tente novamente.");
                    }
                    else
                    {
                        // Outros erros (ex: Password fraca, precisa de letra maiúscula)
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro de base de dados ao registar utilizador");

                // Verificar se é violação de constraint do NIF
                if (ex.InnerException?.Message.Contains("NIF") == true)
                {
                    ModelState.AddModelError("NIF", "Este NIF já está associado a outra conta.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao processar o registo. Por favor, tente novamente.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro inesperado ao registar utilizador");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro inesperado. Por favor, tente novamente.");
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Tentar Login
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Erro inesperado ao obter dados do utilizador.");
                    return View(model);
                }

                // 2. Verificar Bloqueio Global
                if (user.IsBlocked || user.IsDeleted)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Conta bloqueada ou eliminada.");
                    return View(model);
                }

                // 3. Verificar Vendedor
                if (await _userManager.IsInRoleAsync(user, Roles.Vendedor))
                {
                    var vendedor = await _context.Vendedores
                        .Select(v => new { v.UserId, v.Status })
                        .FirstOrDefaultAsync(v => v.UserId == user.Id);

                    if (vendedor != null && vendedor.Status != StatusAprovacao.Aprovado)
                    {
                        _logger.LogWarning("Vendedor {Email} tentou entrar mas está com status {Status}", user.Email, vendedor.Status);

                        return RedirectToAction(nameof(AguardandoAprovacao));
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Conta bloqueada temporariamente devido a tentativas falhadas: {Email}", model.Email);
                return View("Lockout"); 
            }
            ModelState.AddModelError(string.Empty, "Login inválido.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Action para confirmar email (link clicado pelo user)
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) return View("Error", new ErrorViewModel { Message = "Link de confirmação inválido ou incompleto." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("Error", new ErrorViewModel { Message = "Utilizador não encontrado no sistema." });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return View("EmailConfirmado");

            var erros = string.Join(", ", result.Errors.Select(e => e.Description));

            _logger.LogWarning("Falha na confirmação de email para {UserId}: {Errors}", userId, erros);
            return View("Error", new ErrorViewModel
            {
                Message = "Não foi possível confirmar o email. O link pode ter expirado ou já foi utilizado."
                // OPCIONAL: Request Id para debug técnico
                // RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [HttpGet]
        [Authorize(Roles = Roles.Vendedor)]
        public IActionResult AguardandoAprovacao()
        {
            // Vamos buscar o utilizador para garantir que é mesmo um Vendedor
            // (Otimização: Injetar IAuthMessageService aqui se quiseres reenviar email, etc.)
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConta()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // --- SOFT DELETE DO USER ---
                user.IsDeleted = true;
                
                var deleteToken = Guid.NewGuid().ToString();
                // Limpar dados sensíveis para cumprir RGPD
                user.UserName = $"deleted_{deleteToken}";
                user.Email = $"deleted_{deleteToken}@automarket.com";
                user.NormalizedUserName = user.UserName.ToUpper();
                user.NormalizedEmail = user.Email.ToUpper();
                user.NIF = null; // Liberta o NIF para uso futuro

                var resultUser = await _userManager.UpdateAsync(user);
                if (!resultUser.Succeeded)
                {
                    return View("Error", new ErrorViewModel
                    {
                        Message = "Erro ao eliminar conta. Por favor, tente novamente."
                    });
                }

                // --- DESATIVAR VENDEDOR ---
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UserId == user.Id);

                if (vendedor != null)
                {
                    // Mudar status para Rejeitado para impedir novas vendas
                    // garante que os carros não aparecem na listagem
                    // se as queries filtrarem por v.Status == Aprovado
                    vendedor.Status = StatusAprovacao.Rejeitado;
                    vendedor.MotivoRejeicao = "Conta eliminada pelo utilizador.";

                    _context.Vendedores.Update(vendedor);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Logout e Redirect
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Utilizador {Id} apagou a conta / Perfil de vendedor desativado.", user.Id);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao apagar conta do utilizador {Id}", user.Id);
                return View("Error", new ErrorViewModel 
                { 
                    Message = "Ocorreu um erro ao apagar a conta. Tente novamente mais tarde." 
                });
            }
        }
    }
}

