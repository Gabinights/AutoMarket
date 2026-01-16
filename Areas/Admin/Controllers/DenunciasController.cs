using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Area("Admin")]
    public class DenunciasController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IDenunciaService _denunciaService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<DenunciasController> _logger;

        public DenunciasController(
            UserManager<Utilizador> userManager,
            IDenunciaService denunciaService,
            IAuditoriaService auditoriaService,
            ILogger<DenunciasController> logger)
        {
            _userManager = userManager;
            _denunciaService = denunciaService;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Admin/Denuncias/Index
        /// Lista todas as denuncias com filtro por estado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? estado = null, int page = 1)
        {
            try
            {
                var denuncias = await _denunciaService.ListarDenunciasAsync(estado, page, 20);
                var contagens = await _denunciaService.ContarPorEstadoAsync();

                ViewData["Contagens"] = contagens;
                ViewData["EstadoFiltro"] = estado;
                ViewData["CurrentPage"] = page;

                return View(denuncias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar denuncias");
                TempData["Erro"] = "Erro ao carregar denuncias.";
                return View(new List<Denuncia>());
            }
        }

        /// <summary>
        /// GET: Admin/Denuncias/Detalhe/5
        /// Exibe detalhes completos de uma denuncia.
        /// </summary>
        [HttpGet("{id}")]
        [Route("Admin/Denuncias/{id}")]
        public async Task<IActionResult> Detalhe(int id)
        {
            var denuncia = await _denunciaService.ObterDenunciaAsync(id);
            if (denuncia == null)
                return NotFound();

            return View(denuncia);
        }

        /// <summary>
        /// POST: Admin/Denuncias/IniciarAnalise/5
        /// Muda denuncia de "Aberta" para "Em Análise".
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarAnalise(int id)
        {
            var adminId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var sucesso = await _denunciaService.IniciarAnaliseAsync(id, adminId);
            if (!sucesso)
            {
                TempData["Erro"] = "Nao foi possivel iniciar a analise.";
                return RedirectToAction(nameof(Detalhe), new { id });
            }

            TempData["Sucesso"] = "Analise iniciada.";
            return RedirectToAction(nameof(Detalhe), new { id });
        }

        /// <summary>
        /// POST: Admin/Denuncias/Encerrar/5
        /// Encerra denuncia com decisao (Procedente/Nao Procedente).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Encerrar(int id, bool procedente, string decisao)
        {
            var adminId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(decisao))
            {
                TempData["Erro"] = "Decisao nao pode estar vazia.";
                return RedirectToAction(nameof(Detalhe), new { id });
            }

            var sucesso = await _denunciaService.EncerrarDenunciaAsync(id, adminId, procedente, decisao);
            if (!sucesso)
            {
                TempData["Erro"] = "Nao foi possivel encerrar a denuncia.";
                return RedirectToAction(nameof(Detalhe), new { id });
            }

            TempData["Sucesso"] = $"Denuncia encerrada como {(procedente ? "PROCEDENTE" : "NAO PROCEDENTE")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
