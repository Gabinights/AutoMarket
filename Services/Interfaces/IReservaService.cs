using AutoMarket.Models.DTOs;
using AutoMarket.Models.Enums;

namespace AutoMarket.Services.Interfaces
{
    public interface IReservaService
    {
        Task<CommandResultDto> ReservarVeiculoAsync(ReservarVeiculoDto dto, string userId);
    }
}