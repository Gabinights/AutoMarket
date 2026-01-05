using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarPagamento(int veiculoId, string morada, string nif)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId)) 
                    return RedirectToAction("Login", "Conta");

                var transacao = new Transacao
                {
                    VeiculoId = veiculoId,
                    MoradaEnvioSnapshot = morada,
                    NifFaturacaoSnapshot = nif
                };

                var result = await _checkoutService.ProcessCheckoutAsync(userId, transacao);
                return RedirectToAction("Confirmacao", new { id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento");
                return RedirectToAction("Index", "Veiculos");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmacao(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) 
                return RedirectToAction("Login", "Conta");

            var transacao = await _checkoutService.GetTransacaoAsync(userId, id);
            if (transacao == null) 
                return NotFound();

            return View(transacao);
        }
    }
}