using AutoMarket.Models.Constants;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    public class TransacoesController : Controller
    {
        private readonly ITransacaoService _transacaoService;
        private readonly IProfileService _profileService;
        private readonly IVeiculoService _veiculoService;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<TransacoesController> _logger;

        public TransacoesController(ITransacaoService transacaoService, IProfileService profileService, IVeiculoService veiculoService, UserManager<Utilizador> userManager, ILogger<TransacoesController> logger)
        {
            _transacaoService = transacaoService;
            _profileService = profileService;
            _veiculoService = veiculoService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// GET: Transacoes/Checkout/5
        /// Exibe a página de checkout para compra de veículo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Challenge(); }

            if (string.IsNullOrEmpty(user.NIF))
            {
                TempData["ReturnUrl"] = Url.Action(nameof(Checkout), new { id });
                return RedirectToAction("PreencherDadosFiscais", "Conta");
            }

            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id);
            if (veiculo == null)
                return NotFound();

            return View(veiculo);
        }

        /// <summary>
        /// GET: Transacoes/MinhasCompras
        /// Lista todas as transações onde o utilizador atual é o comprador.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MinhasCompras()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);

            if (comprador == null) return View(new List<TransacaoListViewModel>());

            var transacoes = await _transacaoService.GetMinhasComprasAsync(comprador.Id);
            return View(transacoes);
        }

        /// <summary>
        /// GET: Transacoes/MinhasVendas
        /// Lista todas as transações onde o utilizador atual é o vendedor.
        /// Apenas acessível por utilizadores com role Vendedor.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.Vendedor)]
        public async Task<IActionResult> MinhasVendas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendedor = await _profileService.GetVendedorByUserIdAsync(user.Id);

            if (vendedor == null) return View(new List<TransacaoListViewModel>());

            var transacoes = await _transacaoService.GetMinhasVendasAsync(vendedor.Id);
            return View(transacoes);
        }
    }
}
