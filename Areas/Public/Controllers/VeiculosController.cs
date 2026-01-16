using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    /// <summary>
    /// Controller público para listar e pesquisar veículos do catálogo.
    /// Implementa filtros avançados, paginação e otimização de queries.
    /// </summary>
    [Area("Public")]
    public class VeiculosController : Controller
    {
        private readonly IVeiculoService _veiculoService;
        private readonly ILogger<VeiculosController> _logger;

        public VeiculosController(IVeiculoService veiculoService, ILogger<VeiculosController> logger)
        {
            _veiculoService = veiculoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Veiculos/Index
        /// Lista todos os veículos ativos com filtros, ordenação e paginação.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(VeiculoSearchFiltersDto filters)
        {
            // Normalize page
            filters.Page = Math.Max(filters.Page, 1);

            // Use service for Search
            var (veiculos, totaCount, totalPages) = await _veiculoService.SearchVeiculosAsync(filters);

            // Use Service for Filter Options
            var options = await _veiculoService.GetFilterOptionsAsync();

            // Popular View Data
            ViewData["Marcas"] = options.Marcas;
            ViewData["Modelos"] = options.Modelos;
            ViewData["Combustiveis"] = options.Combustiveis;
            ViewData["Anos"] = options.Anos;
            ViewData["Categorias"] = options.Categorias;

            ViewData["MarcaSelecionada"] = filters.Marca;
            ViewData["ModeloSelecionado"] = filters.Modelo;
            ViewData["CombustivelSelecionado"] = filters.Combustivel;
            ViewData["AnoSelecionado"] = filters.Ano;
            ViewData["CategoriaSelecionada"] = filters.Categoria;
            ViewData["PrecoMin"] = filters.PrecoMin;
            ViewData["PrecoMax"] = filters.PrecoMax;
            ViewData["KmMin"] = filters.KmMin;
            ViewData["KmMax"] = filters.KmMax;
            ViewData["Ordenacao"] = filters.Ordenacao;

            // Informações de paginação
            ViewData["CurrentPage"] = filters.Page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totaCount;
            ViewData["PageSize"] = filters.PageSize;

            _logger.LogInformation(
                "Pesquisa de veículos: {TotalVeiculos} encontrados, Página {Page} de {TotalPages}",
                totaCount, filters.Page, totalPages);

            return View(veiculos);
        }

        /// <summary>
        /// POST: Veiculos/BuscarAjax
        /// Retorna PartialView com resultados de busca para AJAX.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BuscarAjax(VeiculoSearchFiltersDto filters)
        {
            filters.Page = Math.Max(filters.Page, 1);
            var (veiculos, totalCount, totalPages) = await _veiculoService.SearchVeiculosAsync(filters);

            ViewData["CurrentPage"] = filters.Page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalCount;
            ViewData["PageSize"] = filters.PageSize;

            return PartialView("_CarListPartial", veiculos);
        }

        /// <summary>
        /// GET: Veiculos/Detalhe/5
        /// Exibe detalhes completos de um veículo.
        /// </summary>
        [HttpGet]
        [Route("Veiculos/Detalhe/{id}")]
        public async Task<IActionResult> Detalhe(int? id)
        {
            if (!id.HasValue) return NotFound();

            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id.Value);

            if (veiculo == null || veiculo.Estado != EstadoVeiculo.Ativo)
                return NotFound();

            return View(veiculo);
        }
    }
}
