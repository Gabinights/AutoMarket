using AutoMarket.Models;

namespace AutoMarket.Services
{
    public interface ICarrinhoService
    {
        /// <summary>
        /// Adiciona um veículo ao carrinho.
        /// </summary>
        void AdicionarItem(Veiculo veiculo);

        /// <summary>
        /// Remove um item do carrinho pelo ID do veículo.
        /// </summary>
        void RemoverItem(int veiculoId);

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