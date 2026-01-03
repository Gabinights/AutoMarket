using AutoMarket.Constants;
using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.DTOs;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models.ViewModels;

namespace AutoMarket.Controllers
{
    /// <summary>
    /// Controller público para listar e pesquisar veículos do catálogo.
    /// Implementa filtros avançados, paginação e otimização de queries.
    /// </summary>
    public class VeiculosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VeiculosController> _logger;

        public VeiculosController(ApplicationDbContext context, ILogger<VeiculosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: Veiculos/Index
        /// Lista todos os veículos ativos com filtros, ordenação e paginação.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? marca = null,
            string? modelo = null,
            string? combustivel = null,
            int? ano = null,
            string? categoria = null,
            decimal? precoMin = null,
            decimal? precoMax = null,
            int? kmMin = null,
            int? kmMax = null,
            string? ordenacao = "recente",
            int page = 1)
        {
            try
            {
                // Encapsular parâmetros no DTO
                var filters = new VeiculoSearchFiltersDto
                {
                    Marca = marca,
                    Modelo = modelo,
                    Combustivel = combustivel,
                    Ano = ano,
                    Categoria = categoria,
                    PrecoMin = precoMin,
                    PrecoMax = precoMax,
                    KmMin = kmMin,
                    KmMax = kmMax,
                    Ordenacao = ordenacao,
                    Page = Math.Max(page, 1) // Garantir que page >= 1
                };

                // Construir query filtrada e ordenada
                var query = BuildVeiculosQuery(filters);

                // Contar total ANTES de paginar
                var totalVeiculos = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalVeiculos / filters.PageSize);

                // Aplicar paginação
                var veiculos = await query
                    .Skip((filters.Page - 1) * filters.PageSize)
                    .Take(filters.PageSize)
                    .ToListAsync();

                // Carregar opções dos filtros em UMA ÚNICA QUERY
                var filterOptions = await LoadFilterOptionsAsync();

                // Passar dados para a view
                ViewData["Marcas"] = filterOptions.Marcas;
                ViewData["Modelos"] = filterOptions.Modelos;
                ViewData["Combustiveis"] = filterOptions.Combustiveis;
                ViewData["Anos"] = filterOptions.Anos;
                ViewData["Categorias"] = filterOptions.Categorias;

                // Manter filtros selecionados
                ViewData["MarcaSelecionada"] = marca;
                ViewData["ModeloSelecionado"] = modelo;
                ViewData["CombustivelSelecionado"] = combustivel;
                ViewData["AnoSelecionado"] = ano;
                ViewData["CategoriaSelecionada"] = categoria;
                ViewData["PrecoMin"] = precoMin;
                ViewData["PrecoMax"] = precoMax;
                ViewData["KmMin"] = kmMin;
                ViewData["KmMax"] = kmMax;
                ViewData["Ordenacao"] = ordenacao;

                // Informações de paginação
                ViewData["CurrentPage"] = filters.Page;
                ViewData["TotalPages"] = totalPages;
                ViewData["TotalItems"] = totalVeiculos;
                ViewData["PageSize"] = filters.PageSize;

                _logger.LogInformation(
                    "Pesquisa de ve�culos: {TotalVeiculos} encontrados, P�gina {Page} de {TotalPages}",
                    totalVeiculos, filters.Page, totalPages);

                return View(veiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pesquisar veículos");
                return View(new List<Veiculo>());
            }
        }

        /// <summary>
        /// POST: Veiculos/BuscarAjax
        /// Retorna PartialView com resultados de busca para AJAX.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BuscarAjax(
            string? marca = null,
            string? modelo = null,
            string? combustivel = null,
            int? ano = null,
            string? categoria = null,
            decimal? precoMin = null,
            decimal? precoMax = null,
            int? kmMin = null,
            int? kmMax = null,
            string? ordenacao = "recente",
            int page = 1)
        {
            try
            {
                // Encapsular parâmetros no DTO
                var filters = new VeiculoSearchFiltersDto
                {
                    Marca = marca,
                    Modelo = modelo,
                    Combustivel = combustivel,
                    Ano = ano,
                    Categoria = categoria,
                    PrecoMin = precoMin,
                    PrecoMax = precoMax,
                    KmMin = kmMin,
                    KmMax = kmMax,
                    Ordenacao = ordenacao,
                    Page = Math.Max(page, 1)
                };

                // Construir query filtrada e ordenada
                var query = BuildVeiculosQuery(filters);

                // Contar total ANTES de paginar
                var totalVeiculos = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalVeiculos / filters.PageSize);

                // Aplicar paginação
                var veiculos = await query
                    .Skip((filters.Page - 1) * filters.PageSize)
                    .Take(filters.PageSize)
                    .ToListAsync();

                // Passar dados para a PartialView
                ViewData["CurrentPage"] = filters.Page;
                ViewData["TotalPages"] = totalPages;
                ViewData["TotalItems"] = totalVeiculos;
                ViewData["PageSize"] = filters.PageSize;

                _logger.LogInformation(
                    "Busca AJAX: {TotalVeiculos} veículos, Página {Page} de {TotalPages}",
                    totalVeiculos, filters.Page, totalPages);

                // Retornar PartialView (sem layout)
                return PartialView("_CarListPartial", veiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar ve�culos via AJAX");
                return PartialView("_CarListPartial", new List<Veiculo>());
            }
        }

        /// <summary>
        /// GET: Veiculos/Detalhe/5
        /// Exibe detalhes completos de um veículo.
        /// </summary>
        [HttpGet]
        [Route("Veiculos/Detalhe/{id}")]
        public async Task<IActionResult> Detalhe(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .Include(v => v.Vendedor)
                .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id && v.Estado == EstadoVeiculo.Ativo);

            if (veiculo == null)
                return NotFound();

            return View(veiculo);
        }

        // ============================================================
        // MÉTODOS PRIVADOS (DRY - Reutilizáveis)
        // ============================================================

        /// <summary>
        /// Constrói a query de veículos com filtros e ordenação aplicados.
        /// Reutilizável entre diferentes métodos.
        /// </summary>
        private IQueryable<Veiculo> BuildVeiculosQuery(VeiculoSearchFiltersDto filters)
        {
            var query = _context.Veiculos
                .Where(v => v.Estado == EstadoVeiculo.Ativo)
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .AsQueryable();

            // Aplicar filtros condicionalmente
            if (!string.IsNullOrEmpty(filters.Marca))
                query = query.Where(v => v.Marca == filters.Marca);

            if (!string.IsNullOrEmpty(filters.Modelo))
                query = query.Where(v => v.Modelo == filters.Modelo);

            if (!string.IsNullOrEmpty(filters.Combustivel))
                query = query.Where(v => v.Combustivel == filters.Combustivel);

            if (filters.Ano.HasValue)
                query = query.Where(v => v.Ano == filters.Ano);

            if (!string.IsNullOrEmpty(filters.Categoria))
                query = query.Where(v => v.Categoria.Nome == filters.Categoria);

            if (filters.PrecoMin.HasValue)
                query = query.Where(v => v.Preco >= filters.PrecoMin);

            if (filters.PrecoMax.HasValue)
                query = query.Where(v => v.Preco <= filters.PrecoMax);

            if (filters.KmMin.HasValue)
                query = query.Where(v => v.Km >= filters.KmMin);

            if (filters.KmMax.HasValue)
                query = query.Where(v => v.Km <= filters.KmMax);

            // Aplicar ordenação
            query = filters.Ordenacao switch
            {
                "preco_asc" => query.OrderBy(v => v.Preco),
                "preco_desc" => query.OrderByDescending(v => v.Preco),
                "km_asc" => query.OrderBy(v => v.Km),
                "km_desc" => query.OrderByDescending(v => v.Km),
                "ano_asc" => query.OrderBy(v => v.Ano),
                "ano_desc" => query.OrderByDescending(v => v.Ano),
                _ => query.OrderByDescending(v => v.DataCriacao)
            };

            return query;
        }

        /// <summary>
        /// Carrega todas as opções de filtro em UMA ÚNICA QUERY otimizada.
        /// Evita N+1 queries problem.
        /// </summary>
        private async Task<VeiculoFilterOptionsDto> LoadFilterOptionsAsync()
        {
            // Carregar todos os dados numa única query
            var veiculosAtivos = await _context.Veiculos
                .Where(v => v.Estado == EstadoVeiculo.Ativo)
                .Include(v => v.Categoria)
                .ToListAsync();

            var options = new VeiculoFilterOptionsDto
            {
                Marcas = veiculosAtivos
                    .Select(v => v.Marca)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList(),

                Modelos = veiculosAtivos
                    .Select(v => v.Modelo)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList(),

                Combustiveis = veiculosAtivos
                    .Where(v => !string.IsNullOrEmpty(v.Combustivel))
                    .Select(v => v.Combustivel)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList(),

                Anos = veiculosAtivos
                    .Select(v => v.Ano)
                    .Distinct()
                    .OrderByDescending(a => a)
                    .ToList(),

                Categorias = veiculosAtivos
                    .Where(v => v.Categoria != null)
                    .Select(v => v.Categoria.Nome)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList()
            };

            return options;
        }
    }

    /// <summary>
    /// DTO para retornar opções de filtro.
    /// </summary>
    internal class VeiculoFilterOptionsDto
    {
        public List<string> Marcas { get; set; } = new();
        public List<string> Modelos { get; set; } = new();
        public List<string> Combustiveis { get; set; } = new();
        public List<int> Anos { get; set; } = new();
        public List<string> Categorias { get; set; } = new();
    }
}
