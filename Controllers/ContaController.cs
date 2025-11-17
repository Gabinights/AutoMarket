using AutoMarket.Models;
using AutoMarket.Models.ViewModels;
using AutoMarket.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutoMarket.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="ContaController"/> with the user manager, sign-in manager, and application database context required for account operations.
        /// </summary>
        public ContaController(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        /// Displays the registration page for creating a new user account.
        /// </summary>
        /// <returns>A view result that renders the registration form.</returns>
        [HttpGet]
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new Utilizador
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                Morada = model.Morada,
                Contactos = model.Contactos,
            };

            if (model.TipoConta == "Vendedor")
            {
                user.StatusAprovacao = "Pendente";
            }
            else // Comprador
            {
                user.StatusAprovacao = "Aprovado";
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (model.TipoConta == "Comprador")
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else // Vendedor
                {
                    TempData["MensagemStatus"] = "Conta de vendedor criada com sucesso! Aguarde aprovação para começar a vender.";
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
        /// Indicates that a seller account is awaiting administrator approval.
        /// </summary>
        /// <returns>A ContentResult containing a plain-text message stating the seller account is pending administrator approval.</returns>
        [HttpGet]
        public IActionResult AguardarAprovacao()
        {
            return Content("Conta criada como vendedor. Aguarda aprovação do administrador.");
        }

        /// <summary>
        /// Displays the login view.
        /// </summary>
        /// <returns>The view result for the user login page.</returns>
        [HttpGet]
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Login inválido.");
                    return View(model);
                }
                if (user.StatusAprovacao != "Aprovado")
                {
                    await _signInManager.SignOutAsync();
                    string msg = user.StatusAprovacao == "Pendente"
                        ? "Aguardando aprovação do administrador."
                        : "Conta de vendedor rejeitada.";
                    ModelState.AddModelError(string.Empty, msg);
                    return View(model);
                }
                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada devido a tentativas inválidas. Tente novamente mais tarde.");
                return View(model);
            }
            ModelState.AddModelError(string.Empty, "Login inválido.");
            return View(model);
        }

        /// <summary>
        /// Signs the current user out and redirects to the Home controller's Index action.
        /// </summary>
        /// <returns>A redirect to the Home controller's Index action.</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}