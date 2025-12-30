using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
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
        /// GET: Veiculos/Pesquisa
        /// Exibe a página de pesquisa e filtros de veículos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? marca = null,
            string? modelo = null,
            string? cor = null,
            string? combustivel = null,
            int? ano = null,
            string? categoria = null,
            decimal? precoMin = null,
            decimal? precoMax = null,
            int? kmMin = null,
            int? kmMax = null,
            string? ordenacao = "recente")
        {
            try
            {
                // Início da query - apenas veículos ativos
                var query = _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(marca))
                    query = query.Where(v => v.Marca == marca);

                if (!string.IsNullOrEmpty(modelo))
                    query = query.Where(v => v.Modelo == modelo);

                if (!string.IsNullOrEmpty(cor))
                    query = query.Where(v => v.Cor == cor);

                if (!string.IsNullOrEmpty(combustivel))
                    query = query.Where(v => v.Combustivel == combustivel);

                if (ano.HasValue)
                    query = query.Where(v => v.Ano == ano);

                if (!string.IsNullOrEmpty(categoria))
                    query = query.Where(v => v.Categoria == categoria);

                if (precoMin.HasValue)
                    query = query.Where(v => v.Preco >= precoMin);

                if (precoMax.HasValue)
                    query = query.Where(v => v.Preco <= precoMax);

                if (kmMin.HasValue)
                    query = query.Where(v => v.Quilometros >= kmMin);

                if (kmMax.HasValue)
                    query = query.Where(v => v.Quilometros <= kmMax);

                // Aplicar ordenação
                query = ordenacao switch
                {
                    "preco_asc" => query.OrderBy(v => v.Preco),
                    "preco_desc" => query.OrderByDescending(v => v.Preco),
                    "km_asc" => query.OrderBy(v => v.Quilometros),
                    "km_desc" => query.OrderByDescending(v => v.Quilometros),
                    "ano_asc" => query.OrderBy(v => v.Ano),
                    "ano_desc" => query.OrderByDescending(v => v.Ano),
                    _ => query.OrderByDescending(v => v.DataCriacao) // recente (padrão)
                };

                var veiculos = await query.ToListAsync();

                // Carregar listas de opções para os filtros
                var marcas = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo)
                    .Select(v => v.Marca)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();

                var modelos = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo)
                    .Select(v => v.Modelo)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();

                var cores = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo && !string.IsNullOrEmpty(v.Cor))
                    .Select(v => v.Cor)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var combustiveis = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo && !string.IsNullOrEmpty(v.Combustivel))
                    .Select(v => v.Combustivel)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var anos = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo && v.Ano.HasValue)
                    .Select(v => v.Ano)
                    .Distinct()
                    .OrderByDescending(a => a)
                    .ToListAsync();

                var categorias = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo && !string.IsNullOrEmpty(v.Categoria))
                    .Select(v => v.Categoria)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                // Passar dados para a view
                ViewData["Marcas"] = marcas;
                ViewData["Modelos"] = modelos;
                ViewData["Cores"] = cores;
                ViewData["Combustiveis"] = combustiveis;
                ViewData["Anos"] = anos;
                ViewData["Categorias"] = categorias;
                ViewData["MarcaSelecionada"] = marca;
                ViewData["ModeloSelecionado"] = modelo;
                ViewData["CorSelecionada"] = cor;
                ViewData["CombustivelSelecionado"] = combustivel;
                ViewData["AnoSelecionado"] = ano;
                ViewData["CategoriaSelecionada"] = categoria;
                ViewData["PrecoMin"] = precoMin;
                ViewData["PrecoMax"] = precoMax;
                ViewData["KmMin"] = kmMin;
                ViewData["KmMax"] = kmMax;
                ViewData["Ordenacao"] = ordenacao;

                return View(veiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pesquisar veículos");
                return View(new List<Veiculo>());
            }
        }
    }
}
