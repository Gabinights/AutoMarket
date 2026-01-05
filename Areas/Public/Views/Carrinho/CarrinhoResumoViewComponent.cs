using AutoMarket.Models.ViewModels;
using AutoMarket.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Areas.Public.Views.Cart
{
    // O nome da classe deve terminar em ViewComponent
    public class CarrinhoResumoViewComponent : ViewComponent
    {
        private readonly ICarrinhoService _carrinhoService;

        public CarrinhoResumoViewComponent(ICarrinhoService carrinhoService)
        {
            _carrinhoService = carrinhoService;
        }

        public IViewComponentResult Invoke()
        {
            // Lógica: Vai buscar os dados à sessão via serviço
            var model = new CarrinhoWidgetViewModel
            {
                QuantidadeItens = _carrinhoService.GetContagem(),
                ValorTotal = _carrinhoService.GetTotal()
            };

            // Retorna a View associada a este componente
            return View(model);
        }
    }
}
