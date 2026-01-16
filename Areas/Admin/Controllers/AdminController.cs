using AutoMarket.Models.Constants;
using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IVendedorService _vendedorService;
        private readonly ITransacaoService _transacaoService;
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly IEstatisticasService _estatisticasService;

        public AdminController(
            IVendedorService vendedorService,
            ITransacaoService transacaoService,
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            IEstatisticasService estatisticasService)
        {
            _vendedorService = vendedorService;
            _transacaoService = transacaoService;
            _userManager = userManager;
            _signInManager = signInManager;
            _estatisticasService = estatisticasService;
        }

        // GET: Admin/Dashboard
        [HttpGet]
        [Route("Admin/Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var stats = await _estatisticasService.ObterEstatisticasAsync();
            return View(stats);
        }

        // GET: Admin/Index (Lista de Pendentes)
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var (vendedores, totalCount) = await _vendedorService.GetVendedoresPendentesAsync(page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(vendedores);
        }

        // POST: Admin/Aprovar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(int id)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId == null) return Unauthorized();

            var userId = await _vendedorService.AprovarVendedorAsync(id, adminId);
            if (userId == null) return NotFound();

            // Refresh do cookie para atualizar as claims (StatusVendedor)
            await RefreshUserClaims(userId);

            TempData["MensagemStatus"] = "Vendedor aprovado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Rejeitar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id, string motivo)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(motivo))
            {
                motivo = "Documentação insuficiente ou dados incorretos.";
            }

            var userId = await _vendedorService.RejeitarVendedorAsync(id, adminId, motivo);
            if (userId == null) return NotFound();

            await RefreshUserClaims(userId);

            TempData["MensagemStatus"] = "Vendedor rejeitado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/HistoricoTransacoes
        public async Task<IActionResult> HistoricoTransacoes(int page = 1)
        {
            const int pageSize = 20;

            var (transacoes, totalCount) = await _transacaoService.GetHistoricoTransacoesAsync(page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(transacoes);
        }

        private async Task RefreshUserClaims(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }
    }
}