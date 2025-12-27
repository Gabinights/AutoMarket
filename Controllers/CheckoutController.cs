using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using AutoMarket.Models.ViewModels;
using AutoMarket.Services;
using AutoMarket.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
            if (_carrinhoService.GetContagem() == 0) return RedirectToAction("Index", "Carrinho");

            // 1. Validação inicial do Modelo (inclui a lógica do NIF que definimos antes)
            if (!ModelState.IsValid)
            {
                model.ValorTotal = _carrinhoService.GetTotal();
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Conta");

            // 2. Obter o Perfil de Comprador
            // (O User é o login, o Comprador é a entidade de negócio. Precisamos do ID do Comprador)
            var comprador = await _context.Compradores
                                          .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
            {
                comprador = new Comprador { UserId = user.Id };
                _context.Add(comprador);
                await _context.SaveChangesAsync();
            }

            // 3. Obter o Carro (O ID deve vir do ViewModel, Hidden Input ou CarrinhoService)
            // Assumindo que tens o CarroId no model ou serviço
            // var carroId = model.CarroId; ou _carrinhoService.GetCarroId();
            // Exemplo usando um serviço de carrinho fictício:
            var itemCarrinho = _carrinhoService.GetItens().FirstOrDefault();
            if (itemCarrinho == null) return RedirectToAction("Index", "Carrinho");

            // 4. Criar a Transação (Mapeamento)
            var transacao = new Transacao
            {
                DataTransacao = DateTime.UtcNow,
                ValorTotal = itemCarrinho.Preco,
                MetodoPagamento = model.MetodoPagamento, // Vem do CheckoutViewModel
                Estado = EstadoTransacao.Pendente,

                // RELAÇÕES
                CompradorId = comprador.Id,
                CarroId = itemCarrinho.CarroId,

                // SNAPSHOTS (O segredo da faturação correta)
                // Se pediu fatura com NIF, usamos os dados do form. Se não, usamos dados genéricos ou do user.
                NomeFaturacao = model.QueroFaturaComNif && !string.IsNullOrEmpty(model.NomeFaturacao)
                                ? model.NomeFaturacao
                                : model.NomeCompleto,

                NifFaturacao = model.QueroFaturaComNif
                               ? model.NifFaturacao
                               : null, // Null = Consumidor Final

                MoradaSnapshot = $"{model.Morada}, {model.CodigoPostal}",
                Observacoes = "Compra online"
            };

            // 5. (Opcional) Atualizar o Perfil do User "Silenciosamente"
            // Guardamos o NIF no perfil para a próxima vez ser automático
            if (model.QueroFaturaComNif && string.IsNullOrEmpty(user.NIF) && !string.IsNullOrEmpty(model.NifFaturacao))
            {
                user.NIF = model.NifFaturacao;
                // Não validamos o resultado aqui para não bloquear a venda por erro de perfil
                await _userManager.UpdateAsync(user);
            }

            // 6. Persistência
            try
            {
                _context.Transacoes.Add(transacao);

                // Importante: Marcar o carro como "Reservado" para ninguém comprar ao mesmo tempo
                var carroDb = await _context.Carros.FindAsync(itemCarrinho.CarroId);
                if (carroDb != null) carroDb.Estado = EstadoCarro.Reservado;

                await _context.SaveChangesAsync();

                // Limpar carrinho
                _carrinhoService.LimparCarrinho();

                // Redirecionar para página de Sucesso/Pagamento
                return RedirectToAction("Sucesso", new { id = transacao.Id });
            }
            catch (Exception ex)
            {
                // Log do erro real
                ModelState.AddModelError("", "Erro ao processar a encomenda. Tente novamente.");
                model.ValorTotal = _carrinhoService.GetTotal();
                return View("Index", model);
            }
        }   
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

            return View(id);
        }
    }
}