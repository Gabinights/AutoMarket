using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Options;
using AutoMarket.Models.DTOs;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AutoMarket.Services.Implementations
{
    public class ReservaService : IReservaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ReservationOptions _options;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(
            ApplicationDbContext context,
            IOptions<ReservationOptions> options,
            ILogger<ReservaService> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<CommandResultDto> ReservarVeiculoAsync(ReservarVeiculoDto dto, string userId)
        {
            var errors = new List<string>();

            var comprador = await _context.Compradores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (comprador == null)
            {
                errors.Add("Comprador não encontrado.");
                return new CommandResultDto(false, errors);
            }

            // Anti-spam: limite por comprador
            var reservasAtivasDoComprador = await _context.Reservas
                .CountAsync(r => r.CompradorId == comprador.Id && r.Estado == EstadoReserva.Ativa);
            if (reservasAtivasDoComprador >= _options.MaxReservasAtivasPorComprador)
            {
                errors.Add("Limite de reservas ativas atingido.");
                return new CommandResultDto(false, errors);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var veiculo = await _context.Veiculos
                    .FirstOrDefaultAsync(v => v.Id == dto.VeiculoId);

                if (veiculo == null)
                {
                    errors.Add("Veículo não encontrado.");
                    return new CommandResultDto(false, errors);
                }

                if (veiculo.Estado != EstadoVeiculo.Ativo)
                {
                    errors.Add("Veículo não está disponível.");
                    return new CommandResultDto(false, errors);
                }

                // Checar reserva ativa existente (reforçado por índice único filtrado)
                var reservaAtiva = await _context.Reservas
                    .FirstOrDefaultAsync(r => r.VeiculoId == dto.VeiculoId && r.Estado == EstadoReserva.Ativa);

                if (reservaAtiva != null)
                {
                    errors.Add("Veículo já está reservado.");
                    return new CommandResultDto(false, errors);
                }

                var novaReserva = new Reserva
                {
                    VeiculoId = veiculo.Id,
                    CompradorId = comprador.Id,
                    DataExpiracao = DateTime.UtcNow.AddHours(_options.TempoExpiracaoHoras),
                    Estado = EstadoReserva.Ativa
                };

                veiculo.Estado = EstadoVeiculo.Reservado;

                _context.Reservas.Add(novaReserva);
                _context.Veiculos.Update(veiculo);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CommandResultDto(true, Array.Empty<string>());
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao reservar veículo {VeiculoId} para user {UserId}", dto.VeiculoId, userId);
                errors.Add("Não foi possível reservar o veículo.");
                return new CommandResultDto(false, errors);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao reservar veículo {VeiculoId} para user {UserId}", dto.VeiculoId, userId);
                errors.Add("Erro interno ao processar a reserva.");
                return new CommandResultDto(false, errors);
            }
        }
    }
}