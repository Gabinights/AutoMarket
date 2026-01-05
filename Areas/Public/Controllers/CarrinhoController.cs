using AutoMarket.Infrastructure.Data;
using AutoMarket.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    [Area("Public")]
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoService _carrinhoService;
        private readonly ApplicationDbContext _context;

        public CarrinhoController(ICarrinhoService carrinhoService, ApplicationDbContext context)
        {
            _carrinhoService = carrinhoService;
            _context = context;
        }

        public IActionResult Index()
        {
            var itens = _carrinhoService.GetItens();
            ViewBag.Total = _carrinhoService.GetTotal();
            return View(itens);
        }

        public async Task<IActionResult> Adicionar(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null) return NotFound();

            if (veiculo.Estado != Models.Enums.EstadoVeiculo.Ativo)
            {
                TempData["Erro"] = "Este veículo não está disponível para compra.";
                return RedirectToAction("Index", "Veiculos"); // Ou volta para onde estava
            }

            _carrinhoService.AdicionarItem(veiculo);

            TempData["Sucesso"] = "Veículo adicionado ao carrinho!";
            return RedirectToAction("Index"); // Ou volta para a lista de veículos
        }

        public IActionResult Remover(int id)
        {
            _carrinhoService.RemoverItem(id);
            return RedirectToAction("Index");
        }
    }
}