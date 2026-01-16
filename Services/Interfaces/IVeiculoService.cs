using AutoMarket.Models.ViewModels.Veiculos;

namespace AutoMarket.Services.Interfaces
{
    public interface IVeiculoService
    {
        // --- Queries ---
        Task<Veiculo?> GetVeiculoEntityAsync(int id);
        Task<VeiculoDto?> GetVeiculoByIdAsync(int id);
        Task<List<Veiculo>> GetVeiculosByVendedorAsync(int vendedorId);

        // --- Search & Filters ---
        Task<(List<Veiculo> Veiculos, int TotalCount, int TotalPages)> SearchVeiculosAsync(VeiculoSearchFiltersDto filters);
        Task<VeiculoFilterOptionsDto> GetFilterOptionsAsync();

        // --- CRUD Operations ---
        Task<List<Categoria>> GetCategoriasAsync();
        Task<Veiculo> CreateVeiculoAsync(CreateVeiculoViewModel model, int vendedorId, List<string> imagePaths);
        Task<bool> UpdateVeiculoAsync(int id, EditVeiculoViewModel model, int vendedorId, List<string> imagePaths);
        Task<bool> SoftDeleteVeiculoAsync(int id, int vendedorId);

        // --- Validation ---
        Task<bool> VeiculoBelongsToVendedorAsync(int veiculoId, int vendedorId);
        Task<bool> IsVeiculoVendidoAsync(int veiculoId);

        // --- Admin ---
        Task<(List<Veiculo> Veiculos, int TotalCount)> GetVeiculosParaModeracaoAsync(string? estado, int page, int pageSize);
        Task<bool> PausarVeiculoAdminAsync(int id);
        Task<bool> RemoverVeiculoAdminAsync(int id);
    }
}