using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    public interface IVendedorService
    {
        Task<(List<Vendedor> Vendedores, int TotalCount)> GetVendedoresPendentesAsync(int page, int pageSize);
        Task<Vendedor?> GetVendedorByIdAsync(int id);
        Task<string?> AprovarVendedorAsync(int id, string adminId);
        Task<string?> RejeitarVendedorAsync(int id, string adminId, string motivo);
    }
}