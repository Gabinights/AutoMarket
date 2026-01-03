using AutoMarket.Constants;
using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [Authorize]
    public class TransacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<TransacoesController> _logger;

        public TransacoesController(ApplicationDbContext context, UserManager<Utilizador> userManager, ILogger<TransacoesController> logger)
        {
            _context = context;
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
                TempData["ReturnUrl"] = Url.Action(nameof(Checkout), new { id = id });
                return RedirectToAction("PreencherDadosFiscais", "Conta");
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.Vendedor)
                .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

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

            // Obter perfil de Comprador
            var comprador = await _context.Compradores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
            {
                // Utilizador não tem perfil de comprador ainda
                return View(new List<TransacaoListViewModel>());
            }

            // Buscar transações onde é comprador
            var transacoes = await _context.Transacoes
                .AsNoTracking()
                .Include(t => t.Veiculo)
                    .ThenInclude(v => v.Imagens)
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.User)
                .Where(t => t.CompradorId == comprador.Id)
                .OrderByDescending(t => t.DataTransacao)
                .Select(t => new TransacaoListViewModel
                {
                    TransacaoId = t.Id,
                    DataTransacao = t.DataTransacao,
                    ValorPago = t.ValorPago,
                    Estado = t.Estado,
                    Metodo = t.Metodo,
                    VeiculoId = t.VeiculoId,
                    VeiculoTitulo = t.Veiculo.Titulo,
                    VeiculoMarca = t.Veiculo.Marca,
                    VeiculoModelo = t.Veiculo.Modelo,
                    VeiculoImagemCapa = t.Veiculo.Imagens.FirstOrDefault(i => i.IsCapa).CaminhoFicheiro,
                    VendedorNome = t.Vendedor.User.Nome,
                    MoradaEnvioSnapshot = t.MoradaEnvioSnapshot,
                    NifFaturacaoSnapshot = t.NifFaturacaoSnapshot
                })
                .ToListAsync();

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

            // Obter perfil de Vendedor
            var vendedor = await _context.Vendedores
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendedor == null)
            {
                // Utilizador não tem perfil de vendedor
                return View(new List<TransacaoListViewModel>());
            }

            // Buscar transações onde é vendedor
            var transacoes = await _context.Transacoes
                .AsNoTracking()
                .Include(t => t.Veiculo)
                    .ThenInclude(v => v.Imagens)
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.User)
                .Where(t => t.VendedorId == vendedor.Id)
                .OrderByDescending(t => t.DataTransacao)
                .Select(t => new TransacaoListViewModel
                {
                    TransacaoId = t.Id,
                    DataTransacao = t.DataTransacao,
                    ValorPago = t.ValorPago,
                    Estado = t.Estado,
                    Metodo = t.Metodo,
                    VeiculoId = t.VeiculoId,
                    VeiculoTitulo = t.Veiculo.Titulo,
                    VeiculoMarca = t.Veiculo.Marca,
                    VeiculoModelo = t.Veiculo.Modelo,
                    VeiculoImagemCapa = t.Veiculo.Imagens.FirstOrDefault(i => i.IsCapa).CaminhoFicheiro,
                    CompradorNome = t.Comprador.User.Nome,
                    MoradaEnvioSnapshot = t.MoradaEnvioSnapshot,
                    NifFaturacaoSnapshot = t.NifFaturacaoSnapshot
                })
                .ToListAsync();

            return View(transacoes);
        }
    }
}
