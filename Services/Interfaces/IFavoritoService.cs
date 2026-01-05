using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de favoritos (bookmarks de anúncios).
    /// </summary>
    public interface IFavoritoService
    {
        /// <summary>
        /// Adicionar um veículo aos favoritos de um comprador.
        /// </summary>
        Task<bool> AdicionarFavoritoAsync(int compradorId, int veiculoId);

        /// <summary>
        /// Remover um veículo dos favoritos.
        /// </summary>
        Task<bool> RemoverFavoritoAsync(int compradorId, int veiculoId);

        /// <summary>
        /// Verificar se um veículo está nos favoritos.
        /// </summary>
        Task<bool> EstaNosFavoritosAsync(int compradorId, int veiculoId);

        /// <summary>
        /// Obter lista de veículos favoritos de um comprador.
        /// </summary>
        Task<List<Veiculo>> ListarFavoritosAsync(int compradorId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Contar total de favoritos.
        /// </summary>
        Task<int> ContarFavoritosAsync(int compradorId);
    }
}
