namespace AutoMarket.Services.Interfaces
{
    public interface IVeiculoService
    {
        Task<VeiculoDto?> GetAsync(int id);
        Task<IReadOnlyList<VeiculoDto>> ListAsync(VeiculoFiltroDto filtro);
        Task<CommandResultDto> CreateAsync(VeiculoCreateDto dto);
        Task<CommandResultDto> UpdateAsync(int id, VeiculoUpdateDto dto);
        Task<CommandResultDto> SoftDeleteAsync(int id);
        ValidationResultDto Validate(VeiculoCreateDto dto);
    }
}