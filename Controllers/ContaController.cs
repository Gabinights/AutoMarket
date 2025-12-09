using AutoMarket.Models;
using AutoMarket.Models.ViewModels;
using AutoMarket.Models.Enums;
using AutoMarket.Data;
using AutoMarket.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AutoMarket.Controllers
{
    /// <summary>
    /// Manages account registration, authentication, email confirmation, and logout workflows for AutoMarket users.
    /// </summary>
    public class ContaController : Controller
    {
        private const string TipoContaComprador = "Comprador";
        private const string TipoContaVendedor = "Vendedor";

        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly ILogger<ContaController> _logger;

        public ContaController(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager, IEmailSender emailSender, EmailTemplateService emailTemplateService, ILogger<ContaController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the registration page for creating a new user account.
        /// </summary>
        /// <returns>A view result that renders the registration form.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Processes the registration form, creates a new user with the selected account type, and sets approval status accordingly.
        /// </summary>
        /// <param name="model">The registration data including email, password, name, address, contacts, and account type.</param>
        /// <returns>An IActionResult that redirects to Home/Index when registration succeeds (immediately signing in buyers and showing a pending-approval message for sellers) or returns the registration view populated with validation/errors on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validar explicitamente o TipoConta
            if (model.TipoConta != TipoContaComprador && model.TipoConta != TipoContaVendedor)
            {
                ModelState.AddModelError(string.Empty, $"Tipo de conta inválido. Deve ser '{TipoContaComprador}' ou '{TipoContaVendedor}'.");
                return View(model);
            }

            var user = new Utilizador
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                Morada = model.Morada,
                Contactos = model.Contactos,
                EmailConfirmed = false
            };

            if (model.TipoConta == TipoContaVendedor)
            {
                user.StatusAprovacao = StatusAprovacao.Pendente;
            }
            else // Comprador
            {
                user.StatusAprovacao = StatusAprovacao.Aprovado;
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Gerar token de confirmação de email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // Ensure we have a valid scheme for URL generation
                var scheme = Request.Scheme;
                if (string.IsNullOrEmpty(scheme))
                {
                    scheme = Request.IsHttps ? "https" : "http";
                }

                var confirmationLink = Url.Action("ConfirmarEmail", "Conta", 
                    new { userId = user.Id, token = token }, 
                    scheme, Request.Host.Value);

                if (string.IsNullOrEmpty(confirmationLink))
                {
                    _logger.LogError("Failed to generate confirmation link for user {UserId}. Scheme: {Scheme}, Host: {Host}", 
                        user.Id, scheme, Request.Host.Value);
                    TempData["MensagemStatus"] = "Conta criada, mas houve um erro ao gerar o link de confirmação. Por favor, contacte o suporte.";
                    return RedirectToAction("Index", "Home");
                }

                // Gerar template de email HTML usando Razor view
                string emailBody;
                try
                {
                    emailBody = await _emailTemplateService.GenerateEmailConfirmationTemplateAsync(user.Nome, confirmationLink, HttpContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate email template for user {UserId}. Falling back to static template.", user.Id);
                    // Fallback to static template if Razor view rendering fails
                    try
                    {
                        emailBody = EmailTemplateService.GenerateEmailConfirmationTemplate(user.Nome, confirmationLink);
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Failed to generate fallback email template for user {UserId}", user.Id);
                        TempData["MensagemStatus"] = "Conta criada, mas houve um erro ao gerar o email. Por favor, contacte o suporte.";
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Enviar email de confirmação
                try
                {
                    await _emailSender.SendEmailAsync(
                        user.Email,
                        "Confirme o seu email - AutoMarket",
                        emailBody,
                        HttpContext.RequestAborted
                    );
                    
                    _logger.LogInformation("Email confirmation sent successfully to {Email} for user {UserId}", user.Email, user.Id);
                    TempData["MensagemStatus"] = "Conta criada com sucesso! Por favor, verifique o seu email para confirmar a conta.";
                    return RedirectToAction("Index", "Home");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Email send was cancelled for user {UserId}. Email: {Email}", user.Id, user.Email);
                    TempData["MensagemStatus"] = "Conta criada, mas o envio do email foi cancelado. Por favor, tente solicitar um novo email de confirmação.";
                    return RedirectToAction("Index", "Home");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Email send failed for user {UserId}. Email: {Email}", user.Id, user.Email);
                    TempData["MensagemStatus"] = "Conta criada, mas houve um erro ao enviar o email de confirmação. Por favor, contacte o suporte.";
                    return RedirectToAction("Index", "Home");
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Email authentication failed for user {UserId}. Email: {Email}", user.Id, user.Email);
                    TempData["MensagemStatus"] = "Conta criada, mas houve um erro de autenticação ao enviar o email. Por favor, contacte o suporte.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error sending email for user {UserId}. Email: {Email}", user.Id, user.Email);
                    TempData["MensagemStatus"] = "Conta criada, mas houve um erro ao enviar o email de confirmação. Por favor, contacte o suporte.";
                    return RedirectToAction("Index", "Home");
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        /// <summary>
        /// Action que exibe uma mensagem informando que a conta de vendedor aguarda aprovação do administrador. (TODO: Implementar a parte de administração e interligar com a parte de aprovação de contas)
        /// </summary>
        /// <returns>Um resultado de conteúdo indicando que a conta aguarda aprovação.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AguardarAprovacao()
        {
            return Content("Conta criada como vendedor. Aguarda aprovação do administrador.");
            //TODO: A parte de administração deve ser implementada
            // TODO: Adicionar uma view para exibir a mensagem de aprovação

        }

        /// <summary>
        /// Confirma o email do utilizador a partir de um link enviado por email.
        /// </summary>
        /// <param name="userId">O identificador do utilizador a confirmar.</param>
        /// <param name="token">O token de confirmação emitido pelo Identity.</param>
        /// <returns>Redireciona para Login ou Home com uma mensagem de estado conforme o resultado da confirmação.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmarEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData["MensagemStatus"] = "Link de confirmação inválido.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["MensagemStatus"] = "Utilizador não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            if (user.EmailConfirmed)
            {
                TempData["MensagemStatus"] = "O seu email já foi confirmado. Pode fazer login.";
                return RedirectToAction("Login", "Conta");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["MensagemStatus"] = "Email confirmado com sucesso! Pode agora fazer login.";
                return RedirectToAction("Login", "Conta");
            }
            else
            {
                TempData["MensagemStatus"] = "Erro ao confirmar o email. O link pode ter expirado.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Exibe a página de login para o utilizador.
        /// </summary>
        /// <returns>Uma view que contém o formulário de login.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Authenticates a user using the supplied credentials and enforces seller approval before allowing access.
        /// </summary>
        /// <param name="model">Login view model containing the user's email, password, and remember-me selection.</param>
        /// <returns>Redirects to Home/Index when sign-in succeeds and the account is approved; otherwise returns the login view containing validation errors or a lockout/approval message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            //Fetch user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Perform password sign-in first (this already handles non-existent users)
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                // Only after successful authentication, check email confirmation and approval
                if (!user.EmailConfirmed)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Email não confirmado. Por favor, confirme o seu email antes de fazer login.");
                    return View(model);
                }
                
                if (user.StatusAprovacao != StatusAprovacao.Aprovado)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "A sua conta aguarda aprovação do administrador.");
                    return View(model);
                }

                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada devido a tentativas inválidas. Tente novamente mais tarde.");
                return View(model);
            }
            // Generic error message to avoid revealing whether the email or password was incorrect
            ModelState.AddModelError(string.Empty, "Login inválido.");
            return View(model);
        }

        /// <summary>
        /// Signs the current user out and redirects to the Home controller's Index action.
        /// </summary>
        /// <returns>A redirect to the Home controller's Index action.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}