using AutoMarket.Models.Constants;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
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
            if (!ModelState.IsValid) return View(model);

            // 1. Iniciar Transação
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
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
                        // allow registration to complete, but log the issue
                    }
                    else
                    {
                        await _emailAuthService.EnviarEmailConfirmacaoAsync(user, confirmationLink);
                    }
                    return RedirectToAction("Index", "Home");
                }
                // Se o Identity falhar (ex: password fraca), adicionar erros ao ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                // Se ocorrer um erro, reverter a transação
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao registar utilizador {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Ocorreu um erro interno. Tente novamente.");
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
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

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

            _logger.LogWarning("Falha na confirmação de email para {UserId}: {Errors}",userId, erros);
            return View("Error", new ErrorViewModel
            {
                Message = "Não foi possível confirmar o email. O link pode ter expirado ou já foi utilizado."
                // OPCIONAL: Request Id para debug técnico
                // RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult PreencherDadosFiscais()
        {
            return View(); // Uma view simples com um input "NIF"
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreencherDadosFiscais(DadosFiscaisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }
            user.NIF = model.NIF; // Grava no perfil para sempre

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Se tínhamos uma transação pendente, voltamos para lá
            if (TempData["ReturnUrl"] is string returnUrl)
            {
                TempData.Keep("ReturnUrl");
                return Redirect(returnUrl); // Volta para o checkout
            }
            // Senão, volta para a home
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = Roles.Vendedor)]
        public IActionResult AguardandoAprovacao()
        {
            // Vamos buscar o utilizador para garantir que é mesmo um Vendedor
            // (Otimização: Injetar IAuthMessageService aqui se quiseres reenviar email, etc.)
            return View();
        }

        // GET: Conta/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogWarning("Acesso negado para utilizador {User} ao tentar aceder {Url}", 
                User.Identity?.Name ?? "Anónimo", returnUrl ?? "URL desconhecido");
            return View();
        }

        // GET: Conta/Perfil
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Buscar estatísticas do utilizador
            ViewBag.IsVendedor = await _userManager.IsInRoleAsync(user, Roles.Vendedor);
            ViewBag.IsComprador = await _userManager.IsInRoleAsync(user, Roles.Comprador);

            if (ViewBag.IsComprador)
            {
                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (comprador != null)
                {
                    ViewBag.TotalCompras = await _context.Transacoes
                        .Where(t => t.CompradorId == comprador.Id)
                        .CountAsync();
                }
            }

            if (ViewBag.IsVendedor)
            {
                var vendedor = await _context.Vendedores
                    .FirstOrDefaultAsync(v => v.UserId == user.Id);

                if (vendedor != null)
                {
                    ViewBag.TotalVendas = await _context.Transacoes
                        .Where(t => t.VendedorId == vendedor.Id)
                        .CountAsync();

                    ViewBag.TotalVeiculos = await _context.Veiculos
                        .Where(v => v.VendedorId == vendedor.Id)
                        .CountAsync();

                    ViewBag.StatusVendedor = vendedor.Status.ToString();
                }
            }

            var model = new EditarPerfilViewModel
            {
                Nome = user.Nome,
                Email = user.Email ?? string.Empty,
                Morada = user.Morada,
                Contacto = user.PhoneNumber ?? string.Empty,
                NIF = user.NIF,
                DataRegisto = user.DataRegisto
            };

            return View(model);
        }

        // POST: Conta/Perfil
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(EditarPerfilViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recarregar ViewBag para estatísticas
                var userTemp = await _userManager.GetUserAsync(User);
                if (userTemp == null)
                {
                    ViewBag.IsVendedor = false;
                    ViewBag.IsComprador = false;
                }
                else
                {
                    ViewBag.IsVendedor = await _userManager.IsInRoleAsync(userTemp, Roles.Vendedor);
                    ViewBag.IsComprador = await _userManager.IsInRoleAsync(userTemp, Roles.Comprador);
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Atualizar dados
            user.Nome = model.Nome;
            user.Morada = model.Morada;
            user.PhoneNumber = model.Contacto;
            user.NIF = model.NIF;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["MensagemSucesso"] = "Perfil atualizado com sucesso!";
                _logger.LogInformation("Utilizador {UserId} atualizou o perfil.", user.Id);
                return RedirectToAction(nameof(Perfil));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.IsVendedor = await _userManager.IsInRoleAsync(user, Roles.Vendedor);
            ViewBag.IsComprador = await _userManager.IsInRoleAsync(user, Roles.Comprador);
            return View(model);
        }

        // GET: Conta/AlterarPassword
        [HttpGet]
        [Authorize]
        public IActionResult AlterarPassword()
        {
            return View();
        }

        // POST: Conta/AlterarPassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPassword(AlterarPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.PasswordAtual, model.NovaPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user); // Mantém o user logado
                TempData["MensagemSucesso"] = "Password alterada com sucesso!";
                _logger.LogInformation("Utilizador {UserId} alterou a password.", user.Id);
                return RedirectToAction(nameof(Perfil));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConta()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // LÓGICA DE SOFT DELETE
            // 1. Ofuscar dados pessoais (Opcional, mas recomendado por RGPD)
            // user.Email = $"deleted_{Guid.NewGuid()}@automarket.com";
            // user.UserName = user.Email;
            // user.Nome = "Utilizador Eliminado";
            // user.NIF = null; 

            // 2. Marcar como eliminado
            user.IsDeleted = true;

            // 3. Atualizar na BD
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // 4. Fazer Logout forçado
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Utilizador {Id} apagou a conta (Soft Delete).", user.Id);

                return RedirectToAction("Index", "Home");
            }

            // Tratar erro...
            return View("Perfil");
        }
    }
}


