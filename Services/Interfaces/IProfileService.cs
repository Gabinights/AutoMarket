using AutoMarket.Models.Entities;
using AutoMarket.Models.ViewModels;

namespace AutoMarket.Services.Interfaces
{
    public interface IProfileService
    {
        Task<Comprador?> GetCompradorByUserIdAsync(string userId);
        Task<Vendedor?> GetVendedorByUserIdAsync(string userId);
        Task<PerfilStatsViewModel> GetUserStatsAsync(string userId);
    }
}