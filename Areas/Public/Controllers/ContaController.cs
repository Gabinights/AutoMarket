using System.Security.Claims;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.ViewModels;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    [Area("Public")]
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IAuthService _authService;
        private readonly IProfileService _profileService;
        private readonly IFavoritoService _favoritoService;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<ContaController> _logger;

        public ContaController(
            UserManager<Utilizador> userManager,
            IAuthService authService,
            IProfileService profileService,
            IFavoritoService favoritoService,
            INotificacaoService notificacaoService,
            ILogger<ContaController> logger)
        {
            _userManager = userManager;
            _authService = authService;
            _profileService = profileService;
            _favoritoService = favoritoService;
            _notificacaoService = notificacaoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Conta/Index
        /// Dashboard do perfil do utilizador.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            return View(user);
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
            return Redirect("/"); // Redireciona diretamente para a raiz (Home)
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

        /// <summary>
        /// GET: Conta/Favoritos
        /// Listar veículos favoritos do comprador.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Favoritos(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var comprador = await _profileService.GetCompradorByUserIdAsync(userId);

            if (comprador == null)
                return RedirectToAction("Index", "Home");

            var favoritos = await _favoritoService.ListarFavoritosAsync(comprador.Id, page, 20);
            var total = await _favoritoService.ContarFavoritosAsync(comprador.Id);

            ViewData["TotalFavoritos"] = total;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / 20.0);

            return View(favoritos);
        }

        /// <summary>
        /// GET: Conta/Notificacoes
        /// Listar notificações do utilizador.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Notificacoes(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var notificacoes = await _notificacaoService.ListarNotificacoesAsync(userId, page, 20);

            var total = await _notificacaoService.ContarNotificacoesAsync(userId);

            ViewData["TotalNotificacoes"] = total;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / 20.0);

            return View(notificacoes);
        }




        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Usar o novo ProfileService para obter stats sem context direto
            var stats = await _profileService.GetUserStatsAsync(user.Id);

            ViewBag.IsVendedor = stats.IsVendedor;
            ViewBag.IsComprador = stats.IsComprador;
            ViewBag.TotalCompras = stats.TotalCompras;
            ViewBag.TotalVendas = stats.TotalVendas;
            ViewBag.TotalVeiculos = stats.TotalVeiculos;
            ViewBag.StatusVendedor = stats.StatusVendedor;

            var model = new EditarPerfilViewModel
            {
                Nome = user.Nome,
                Email = user.Email,
                NIF = user.NIF
            };
            return View(model);
        }
    }
}