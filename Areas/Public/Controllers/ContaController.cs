using System.Security.Claims;
using AutoMarket.Models.Constants;
using AutoMarket.Models.ViewModels;

namespace AutoMarket.Areas.Public.Controllers
{
    [Area("Public")]
    public class ContaController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ContaController> _logger;

        public ContaController(
            IAuthService authService,
            ILogger<ContaController> logger)
        {
            _authService = authService;
            _logger = logger;
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

            var confirmationBaseUrl = Url.Action(nameof(ConfirmarEmail), "Conta", null, Request.Scheme) ?? string.Empty;

            var result = await _authService.RegisterAsync(
                new RegisterDto(model.Email, model.Password, model.Nome, model.Morada, model.Contacto, model.NIF, model.TipoConta),
                confirmationBaseUrl);

            if (result.Success)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
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

            var result = await _authService.LoginAsync(new LoginDto(model.Email, model.Password, model.RememberMe));

            if (result.Success)
            {
                return RedirectToAction("Index", "Home");
            }

            if (result.FailureReason == nameof(StatusAprovacao.Pendente))
            {
                return RedirectToAction(nameof(AguardandoAprovacao));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(string userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (result.Success) return View("EmailConfirmado");

            _logger.LogWarning("Falha na confirmação de email para {UserId}: {Errors}", userId, string.Join(", ", result.Errors));
            return View("Error", new ErrorViewModel
            {
                Message = "Não foi possível confirmar o email. O link pode ter expirado ou já foi utilizado."
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult PreencherDadosFiscais()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreencherDadosFiscais(DadosFiscaisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _authService.UpdateNifAsync(userId, model.NIF);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            if (TempData["ReturnUrl"] is string returnUrl)
            {
                TempData.Keep("ReturnUrl");
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = Roles.Vendedor)]
        public IActionResult AguardandoAprovacao()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConta()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var result = await _authService.SoftDeleteAsync(userId);
            if (!result.Success)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Erro ao apagar conta {UserId}: {Error}", userId, error);
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}