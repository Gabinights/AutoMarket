using AutoMarket.Models;
using AutoMarket.Data;
using AutoMarket.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models.ViewModels;

namespace AutoMarket.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender; // <--- Injeção do Serviço
        private readonly EmailTemplateService _emailTemplateService; // <--- Injeção do Template

        public ContaController(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            ApplicationDbContext context,
            IEmailSender emailSender,
            EmailTemplateService emailTemplateService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
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
                        DataRegisto = DateTime.UtcNow
                    };

                    // 2. Criar o User (Identity)
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // 3. Criar o Perfil Específico (Vendedor ou Comprador)
                        if (model.IsVendedor)
                        {
                            var vendedor = new Vendedor
                            {
                                UserId = user.Id,
                                NIF = model.NIF,
                                IsEmpresa = model.IsEmpresa,
                                Status = StatusAprovacao.Pendente
                            };
                            _context.Vendedores.Add(vendedor);

                            await _userManager.AddToRoleAsync(user, "Vendedor");
                        }
                        else
                        {
                            var comprador = new Comprador
                            {
                                UserId = user.Id,
                                ReceberNotificacoes = false
                            };
                            _context.Compradores.Add(comprador);

                            await _userManager.AddToRoleAsync(user, "Comprador");
                        }
                        // 4. Gravar Perfil na BD
                        await _context.SaveChangesAsync();

                        // 5. Se chegámos aqui sem erros, confirmar a transação
                        await transaction.CommitAsync();

                        // =========================================================
                        // Lógica de Envio de Email (Integrada com os Services)
                        // =========================================================
                        try
                        {
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                            // Gera o link completo
                            var confirmationLink = Url.Action("ConfirmarEmail", "Conta",
                                new { userId = user.Id, token = token },
                                Request.Scheme);

                            // Gera o corpo do email usando o serviço existente
                            var emailBody = await _emailTemplateService.GenerateEmailConfirmationTemplateAsync(
                                user.Nome,
                                confirmationLink,
                                this.HttpContext
                            );

                            // Envia o email usando o serviço de transporte (SMTP/Mock)
                            await _emailSender.SendEmailAsync(
                                user.Email,
                                "Confirmação de Conta - AutoMarket",
                                emailBody
                            );
                        }
                        catch (Exception ex)
                        {
                            // TODO: logar o erro (_logger.LogError(ex,...))
                            TempData["EmailWarning"] = "Conta criada, mas ocorreu um erro ao enviar o email de confirmação.";
                        }

                        // Se requer confirmação, redirecionar para aviso
                        // Se não requer (para testes rápidos), fazer login:
                        // await _signInManager.SignInAsync(user, isPersistent: false);

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
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao registar. Por favor, tente novamente.");
                    // TODO: logar o erro (_logger.LogError(ex,...))
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

            // 1. Tentar Login
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                // 2. Verificar Bloqueio Global
                if (user.IsBlocked)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Conta bloqueada por um administrador.");
                    return View(model);
                }

                // 3. Verificar Vendedor
                if (await _userManager.IsInRoleAsync(user, "Vendedor"))
                {
                    // Nota: O Include é importante se quiseres ver detalhes, mas FirstOrDefault chega para o Status
                    var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UserId == user.Id);

                    if (vendedor != null && vendedor.Status != StatusAprovacao.Aprovado)
                    {
                        // Aqui decides: Ou deixas entrar com acesso limitado (Policy) ou bloqueias.
                        // Para já, deixamos entrar. A Policy no CarrosController trata do resto.
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
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("Index", "Home");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return View("EmailConfirmado"); // Cria esta View simples ou redireciona

            return View("Error");
        }
    }
}