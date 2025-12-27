using AutoMarket.Models;

namespace AutoMarket.Services
{
    public interface ICarrinhoService
    {
        /// <summary>
        /// Adiciona um carro ao carrinho.
        /// </summary>
        void AdicionarItem(Carro carro);

        /// <summary>
        /// Remove um item do carrinho pelo ID do carro.
        /// </summary>
        void RemoverItem(int carroId);

        /// <summary>
        /// Obtém a lista atual de itens no carrinho.
        /// </summary>
        List<CarrinhoItem> GetItens();

        /// <summary>
        /// Remove todos os itens do carrinho.
        /// </summary>
        void LimparCarrinho();

        /// <summary>
        /// Calcula o valor total dos itens no carrinho.
        /// </summary>
        decimal GetTotal();

        /// <summary>
        /// Retorna a quantidade de itens no carrinho.
        /// </summary>
        int GetContagem();
    }
}