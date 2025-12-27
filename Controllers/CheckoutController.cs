using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using AutoMarket.Models.ViewModels;
using AutoMarket.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ICarrinhoService _carrinhoService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            ICarrinhoService carrinhoService,
            ILogger<CheckoutController> logger)
        {
            _context = context;
            _userManager = userManager;
            _carrinhoService = carrinhoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Conta");

            var carrinhoItens = _carrinhoService.GetItens();
            if (!carrinhoItens.Any()) return RedirectToAction("Index", "Carrinho");

            var model = new CheckoutViewModel
            {
                // Pré-preenchemos com os dados do perfil para facilitar a vida ao user
                NomeCompleto = user.Nome, 
                Morada = user.Morada ?? string.Empty, // Garante que não é null
                // CodigoPostal não existe no Utilizador, user tem de preencher

                // Se o user já tiver NIF no perfil, ativamos a opção de fatura e preenchemos
                NifFaturacao = user.NIF,
                QueroFaturaComNif = !string.IsNullOrEmpty(user.NIF),
                ValorTotal = _carrinhoService.GetTotal()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarCompra(CheckoutViewModel model)
        {
            var itensCarrinho = _carrinhoService.GetItens();
            if (!itensCarrinho.Any()) return RedirectToAction("Index", "Carrinho");

            if (!ModelState.IsValid)
            {
                model.ValorTotal = _carrinhoService.GetTotal();
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Conta");

            // 1. Iniciar transação de BD
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Obter ou criar comprador
                var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (comprador == null)
                {
                    comprador = new Comprador { UserId = user.Id };
                    _context.Add(comprador);
                    await _context.SaveChangesAsync();
                }

                // 3. Atualizar NIF
                if (model.QueroFaturaComNif && string.IsNullOrEmpty(user.NIF) && !string.IsNullOrEmpty(model.NifFaturacao))
                {
                    user.NIF = model.NifFaturacao;
                    await _userManager.UpdateAsync(user);
                }

                var transacoesIds = new List<int>();

                // 4. Processar cada item do carrinho
                foreach (var item in itensCarrinho)
                {
                    var carroDb = await _context.Carros.FindAsync(item.CarroId);
                    if (carroDb == null || carroDb.Estado != EstadoCarro.Ativo)
                    {
                        throw new InvalidOperationException($"O veículo {item.Marca} {item.Modelo} já não está disponível.");
                    }

                    var transacao = new Transacao
                    {
                        DataTransacao = DateTime.UtcNow,
                        ValorTotal = item.Preco,
                        MetodoPagamento = model.MetodoPagamento,
                        Estado = EstadoTransacao.Pendente,
                        CompradorId = comprador.Id,
                        CarroId = item.CarroId,
                        NomeFaturacao = model.QueroFaturaComNif && !string.IsNullOrEmpty(model.NomeFaturacao)
                                        ? model.NomeFaturacao
                                        : model.NomeCompleto,
                        NifFaturacao = model.QueroFaturaComNif
                                       ? model.NifFaturacao
                                       : null,
                        MoradaSnapshot = $"{model.Morada}, {model.CodigoPostal}",
                        Observacoes = "Compra online"
                    };
                    _context.Transacoes.Add(transacao);
                    carroDb.Estado = EstadoCarro.Reservado;
                    await _context.SaveChangesAsync();
                    transacoesIds.Add(transacao.Id);
                }
                // 5. Commit da transação de BD
                await dbTransaction.CommitAsync();

                _carrinhoService.LimparCarrinho();

                return RedirectToAction("Sucesso", new { id = transacoesIds.First() }); // TODO: ALTERAR ( redirecionar para página de sucesso )
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar compra para o utilizador {UserId}", user.Id);
                ModelState.AddModelError("", ex.Message);
                model.ValorTotal = _carrinhoService.GetTotal();
                return View("Index", model);
            }
        }   

        [HttpGet]
        public async Task<IActionResult> Sucesso(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Conta");

            var transacao = await _context.Transacoes
                .Include(t => t.Comprador)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transacao == null) return NotFound();

            // Verificar se a transação pertence ao user logado
            if (transacao.Comprador.UserId != user.Id)
            {
                return Forbid();
            }
                    
            return View(transacao);
        }
    }
}