using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Area("Admin")]
    public class DenunciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IDenunciaService _denunciaService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<DenunciasController> _logger;

        public DenunciasController(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            IDenunciaService denunciaService,
            IAuditoriaService auditoriaService,
            ILogger<DenunciasController> logger)
        {
            _context = context;
            _userManager = userManager;
            _denunciaService = denunciaService;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Admin/Denuncias/Index
        /// Lista todas as denúncias com filtro por estado.
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
                _logger.LogError(ex, "Erro ao listar denúncias");
                TempData["Erro"] = "Erro ao carregar denúncias.";
                return View(new List<Denuncia>());
            }
        }

        /// <summary>
        /// GET: Admin/Denuncias/Detalhe/5
        /// Exibe detalhes completos de uma denúncia.
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
        /// Muda denúncia de "Aberta" para "Em Análise".
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
                TempData["Erro"] = "Não foi possível iniciar a análise.";
                return RedirectToAction(nameof(Detalhe), new { id });
            }

            TempData["Sucesso"] = "Análise iniciada.";
            return RedirectToAction(nameof(Detalhe), new { id });
        }

        /// <summary>
        /// POST: Admin/Denuncias/Encerrar/5
        /// Encerra denúncia com decisão (Procedente/Não Procedente).
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
                TempData["Erro"] = "Decisão não pode estar vazia.";
                return RedirectToAction(nameof(Detalhe), new { id });
            }

            var sucesso = await _denunciaService.EncerrarDenunciaAsync(id, adminId, procedente, decisao);
            if (!sucesso)
            {
                TempData["Erro"] = "Não foi possível encerrar a denúncia.";
                return RedirectToAction(nameof(Detalhe), new { id });
            }

            TempData["Sucesso"] = $"Denúncia encerrada como {(procedente ? "PROCEDENTE" : "NÃO PROCEDENTE")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
