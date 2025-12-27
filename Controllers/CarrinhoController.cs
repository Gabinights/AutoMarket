using AutoMarket.Data;
using AutoMarket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
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
            var carro = await _context.Carros
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carro == null) return NotFound();

            if (carro.Estado != AutoMarket.Models.Enums.EstadoCarro.Ativo)
            {
                TempData["Erro"] = "Este veículo não está disponível para compra.";
                return RedirectToAction("Index", "Veiculos"); // Ou volta para onde estava
            }

            _carrinhoService.AdicionarItem(carro);

            TempData["Sucesso"] = "Carro adicionado ao carrinho!";
            return RedirectToAction("Index"); // Ou volta para a lista de carros
        }

        public IActionResult Remover(int id)
        {
            _carrinhoService.RemoverItem(id);
            return RedirectToAction("Index");
        }
    }
}