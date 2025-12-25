using AutoMarket.Models;

namespace AutoMarket.Services
{
    public interface ICarrinhoService
    {
        void AdicionarItem(Carro carro);
        void RemoverItem(int carroId);
        List<CarrinhoItem> GetItens();
        void LimparCarrinho();
        decimal GetTotal();
        int GetContagem();
    }
}