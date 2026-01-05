using AutoMarket.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICheckoutService checkoutService,
            UserManager<Utilizador> userManager,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Conta");

            var checkout = await _checkoutService.GetCheckoutAsync(userId);
            if (checkout == null) return RedirectToAction("Index", "Carrinho");

            var model = new CheckoutViewModel
            {
                NomeCompleto = checkout.NomeCompleto,
                Morada = checkout.Morada ?? string.Empty,
                NifFaturacao = checkout.Nif,
                QueroFaturaComNif = !string.IsNullOrEmpty(checkout.Nif),
                ValorTotal = checkout.ValorTotal
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarCompra(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Conta");

            if (!ModelState.IsValid)
            {
                model.ValorTotal = model.ValorTotal == 0 ? model.ValorTotal : model.ValorTotal;
                return View("Index", model);
            }

            var result = await _checkoutService.ProcessAsync(
                userId,
                new CheckoutInputDto(
                    model.Morada,
                    model.CodigoPostal,
                    model.QueroFaturaComNif,
                    model.NifFaturacao,
                    model.MetodoPagamento));

            if (result.Success && result.FirstTransactionId.HasValue)
            {
                return RedirectToAction("Sucesso", new { id = result.FirstTransactionId.Value });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            model.ValorTotal = model.ValorTotal == 0 ? model.ValorTotal : model.ValorTotal;
            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Sucesso(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Conta");

            var transacao = await _checkoutService.GetTransacaoAsync(userId, id);
            if (transacao == null) return NotFound();

            return View(transacao);
        }
    }
}