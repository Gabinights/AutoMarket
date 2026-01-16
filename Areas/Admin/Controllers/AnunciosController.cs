using AutoMarket.Infrastructure.Data;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnunciosController : Controller
    {

        private readonly IVeiculoService _veiculoService;
        private readonly ILogger<AnunciosController> _logger;

        public AnunciosController(IVeiculoService veiculoService, ILogger<AnunciosController> logger)
        {
            _veiculoService = veiculoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Admin/Anuncios
        /// Listar todos os anuncios para moderar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? estado = null, int page = 1)
        {
            try
            {
                const int pageSize = 10;
                var (veiculos, totalCount) = await _veiculoService.GetVeiculosParaModeracaoAsync(estado, page, pageSize);

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (decimal)pageSize);
                ViewData["TotalAnuncios"] = totalCount;
                ViewData["EstadoFiltro"] = estado;

                return View(veiculos);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar anuncios");
                return RedirectToAction("Index", "Admin");
            }
        }

        /// <summary>
        /// POST: Admin/Anuncios/Pausar/5
        /// Pausar um anuncio
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Pausar(int id)
        {
            var result = await _veiculoService.PausarVeiculoAdminAsync(id);
            if (!result) return NotFound();

            _logger.LogInformation("Anuncio {VeiculoId} pausado pelo admin", id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: Admin/Anuncios/Remover/5
        /// Remover um anúncio
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Remover(int id)
        {
            var result = await _veiculoService.RemoverVeiculoAdminAsync(id);
            if (!result) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
