using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnunciosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnunciosController> _logger;

        public AnunciosController(ApplicationDbContext context, ILogger<AnunciosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: Admin/Anuncios
        /// Listar todos os anúncios para moderar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? estado = null, int page = 1)
        {
            try
            {
                var query = _context.Veiculos
                    .Include(v => v.Vendedor)
                    .ThenInclude(v => v.User)
                    .Include(v => v.Imagens)
                    .AsQueryable();

                // Filtro por estado
                if (!string.IsNullOrEmpty(estado))
                    query = query.Where(v => v.Estado.ToString() == estado);

                var total = await query.CountAsync();
                const int pageSize = 10;

                var anuncios = await query
                    .OrderByDescending(v => v.DataCriacao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = (int)Math.Ceiling(total / (decimal)pageSize);
                ViewData["TotalAnuncios"] = total;
                ViewData["EstadoFiltro"] = estado;

                return View(anuncios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar anúncios");
                return RedirectToAction("Index", "Admin");
            }
        }

        /// <summary>
        /// GET: Admin/Anuncios/Pausar/5
        /// Pausar um anúncio
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Pausar(int id)
        {
            try
            {
                var veiculo = await _context.Veiculos.FindAsync(id);
                if (veiculo == null) return NotFound();

                veiculo.Estado = Models.Enums.EstadoVeiculo.Pausado;
                _context.Update(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Anúncio {VeiculoId} pausado pelo admin", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pausar anúncio");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: Admin/Anuncios/Remover/5
        /// Remover um anúncio
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Remover(int id)
        {
            try
            {
                var veiculo = await _context.Veiculos.FindAsync(id);
                if (veiculo == null) return NotFound();

                _context.Remove(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Anúncio {VeiculoId} removido pelo admin", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover anúncio");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
