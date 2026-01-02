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
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ContaController(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            ApplicationDbContext context,
            IEmailAuthService emailAuthService,
            ILogger<ContaController> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _emailAuthService = emailAuthService;
            _environment = environment;
            _configuration = configuration;
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

            if (ModelState.IsValid)
            {
                // 0. Verificar unicidade do NIF antes de começar
                if (!string.IsNullOrEmpty(model.NIF))
                {
                    bool nifExists = await _context.Users.AnyAsync(u => u.NIF == model.NIF);
                    if (nifExists)
                    {
                         ModelState.AddModelError("NIF", "Este NIF já está associado a outra conta.");
                         return View(model);
                    }
                }

                // 1. Iniciar Transação
                using var transaction = await _context.Database.BeginTransactionAsync();

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

                // 2. Criar o User (Identity)
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 3. Criar o Perfil Específico (Vendedor ou Comprador) baseado no enum
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
                    // 4. Gravar Perfil na BD
                    await _context.SaveChangesAsync();
                    // 5. Se chegámos aqui sem erros, confirmar a transação
                    await transaction.CommitAsync();

                    // Lógica de envio de email de confirmação
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action(nameof(ConfirmarEmail), "Conta",
                    new { userId = user.Id, token }, Request.Scheme);

                    if (string.IsNullOrEmpty(confirmationLink))
                    {
                        _logger.LogError("Falha ao gerar link de confirmação para {Email}", user.Email);
                        TempData["ErrorMessage"] = "Erro ao gerar link de confirmação. Por favor, contacte o suporte.";
                        return RedirectToAction(nameof(Login));
                    }

                    // Verificar se email está configurado
                    var smtpConfigured = !string.IsNullOrEmpty(_configuration["EmailSettings:SmtpServer"]) &&
                                        !string.IsNullOrEmpty(_configuration["EmailSettings:SmtpUsername"]) &&
                                        !string.IsNullOrEmpty(_configuration["EmailSettings:SmtpPassword"]);

                    try
                    {
                        await _emailAuthService.EnviarEmailConfirmacaoAsync(user, confirmationLink);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao enviar email de confirmação para {Email}", user.Email);
                    }

                    // Se está em desenvolvimento OU o email não está configurado, mostrar o link diretamente
                    if (_environment.IsDevelopment() || !smtpConfigured)
                    {
                        _logger.LogWarning("Link de confirmação gerado: {Link}", confirmationLink);
                        return View("AguardarConfirmacao", confirmationLink);
                    }

                    // Em produção com email configurado, redirecionar para login com mensagem
                    TempData["SuccessMessage"] = "Registo efetuado com sucesso! Verifique o seu email para confirmar a sua conta.";
                    return RedirectToAction(nameof(Login));
                }
                // Se o Identity falhar (ex: password fraca), adicionar erros ao ModelState
                foreach (var error in result.Errors)
                {
                    // Verificamos os códigos internos do Identity
                    if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                    {
                        // MENSAGEM GENÉRICA: O atacante não sabe se o email existe ou se falhou outra coisa
                        ModelState.AddModelError(string.Empty, "Ocorreu um erro ao processar o registo. Por favor, verifique os dados ou tente novamente.");
                    }
                    else
                    {
                        // Outros erros (ex: Password fraca, precisa de letra maiúscula) 
                        // continuam a ser úteis para o utilizador legítimo corrigir.
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
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

            // Check if user exists first
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Check if email is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogWarning("Tentativa de login com email não confirmado: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Por favor, confirme o seu email antes de fazer login. Verifique a sua caixa de entrada.");
                    return View(model);
                }

                // Check if account is blocked or deleted
                if (user.IsBlocked || user.IsDeleted)
                {
                    ModelState.AddModelError(string.Empty, "Conta bloqueada ou eliminada.");
                    return View(model);
                }
            }

            // 1. Tentar Login
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Erro inesperado ao obter dados do utilizador.");
                    return View(model);
                }

                // 2. Verificar Vendedor
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
            
            ModelState.AddModelError(string.Empty, "Email ou password inválidos.");
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

        [HttpGet]
        public IActionResult ReenviarConfirmacao()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarConfirmacao(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Por favor, introduza o seu email.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            
            // Não revelar se o utilizador existe ou não (segurança)
            if (user == null)
            {
                TempData["SuccessMessage"] = "Se o email existir no sistema, receberá um novo link de confirmação.";
                return RedirectToAction(nameof(Login));
            }

            // Verificar se já está confirmado
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["InfoMessage"] = "Este email já está confirmado. Pode fazer login.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                // Gerar novo token e enviar email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmarEmail), "Conta",
                    new { userId = user.Id, token }, Request.Scheme);

                if (!string.IsNullOrEmpty(confirmationLink))
                {
                    await _emailAuthService.EnviarEmailConfirmacaoAsync(user, confirmationLink);
                    _logger.LogInformation("Email de confirmação reenviado para {Email}", user.Email);
                }

                TempData["SuccessMessage"] = "Se o email existir no sistema, receberá um novo link de confirmação.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reenviar email de confirmação para {Email}", email);
                TempData["SuccessMessage"] = "Se o email existir no sistema, receberá um novo link de confirmação.";
            }

            return RedirectToAction(nameof(Login));
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

