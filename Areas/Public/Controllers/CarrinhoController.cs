using AutoMarket.Services;

namespace AutoMarket.Areas.Public.Controllers
{
    [Area("Public")]
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoService _carrinhoService;
        private readonly IVeiculoService _veiculoService;

        public CarrinhoController(ICarrinhoService carrinhoService, IVeiculoService veiculoService)
        {
            _carrinhoService = carrinhoService;
            _veiculoService = veiculoService;
        }

        public IActionResult Index()
        {
            var itens = _carrinhoService.GetItens();
            ViewBag.Total = _carrinhoService.GetTotal();
            return View(itens);
        }

        public async Task<IActionResult> Adicionar(int id)
        {
            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id);

            if (veiculo == null) return NotFound();

            if (veiculo.Estado != EstadoVeiculo.Ativo)
            {
                TempData["Erro"] = "Veículo indisponível.";
                return RedirectToAction("Index", "Veiculos");
            }

            _carrinhoService.AdicionarItem(veiculo);
            TempData["Sucesso"] = "Adicionado ao carrinho!";
            return RedirectToAction("Index");
        }

        public IActionResult Remover(int id)
        {
            _carrinhoService.RemoverItem(id);
            return RedirectToAction("Index");
        }
    }
}