using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Service para gest�o de reservas de ve�culos.
    /// Controla o ciclo de vida: Cria��o, Confirma��o, Expira��o, Compra.
    /// </summary>
    public class ReservaService : IReservaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(ApplicationDbContext context, ILogger<ReservaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Criar uma reserva de veiculo.
        /// Valida disponibilidade e muda o estado do veiculo para "Reservado".
        /// </summary>
        public async Task<(bool sucesso, Reserva? reserva, string mensagem)> CriarReservaAsync(
            int veiculoId, 
            int compradorId, 
            int diasValidez = 7)
        {
            try
            {
                // 1. Validar veiculo existe
                var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == veiculoId);
                if (veiculo == null)
                    return (false, null, "Veiculo nao encontrado.");

                // 2. Validar veiculo disponivel
                if (veiculo.Estado != EstadoVeiculo.Ativo)
                    return (false, null, $"Veiculo nao disponivel. Estado atual: {veiculo.Estado}");

                // 3. Validar comprador existe
                var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id == compradorId);
                if (comprador == null)
                    return (false, null, "Perfil de comprador nao encontrado.");

                // 4. Validar se ja tem reserva ativa para este veiculo
                var reservaExistente = await _context.Reservas
                    .FirstOrDefaultAsync(r => r.VeiculoId == veiculoId 
                        && r.CompradorId == compradorId
                        && r.Valida);
                if (reservaExistente != null)
                    return (false, null, "Já tem uma reserva ativa para este veículo.");

                // 5. Criar a reserva
                var reserva = new Reserva
                {
                    VeiculoId = veiculoId,
                    CompradorId = compradorId,
                    DataCriacao = DateTime.UtcNow,
                    DataExpiracao = DateTime.UtcNow.AddDays(diasValidez),
                    Estado = EstadoReserva.Pendente
                };

                // 6. Mudar estado do veiculo para Reservado
                veiculo.Estado = EstadoVeiculo.Reservado;

                // 7. Guardar
                _context.Reservas.Add(reserva);
                _context.Veiculos.Update(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Reserva criada: VeiculoId={VeiculoId}, CompradorId={CompradorId}, Validade={DataExpiracao}",
                    veiculoId, compradorId, reserva.DataExpiracao);

                return (true, reserva, "Reserva criada com sucesso! Válida até " + reserva.DataExpiracao.ToShortDateString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar reserva: VeiculoId={VeiculoId}, CompradorId={CompradorId}",
                    veiculoId, compradorId);
                return (false, null, "Erro ao criar reserva. Tente novamente.");
            }
        }

        /// <summary>
        /// Cancelar uma reserva.
        /// Muda o veiculo para "Ativo" novamente (se nao houver outras reservas).
        /// </summary>
        public async Task<(bool sucesso, string mensagem)> CancelarReservaAsync(
            int reservaId, 
            string motivo = "")
        {
            try
            {
                // 1. Encontrar reserva
                var reserva = await _context.Reservas
                    .Include(r => r.Veiculo)
                    .FirstOrDefaultAsync(r => r.Id == reservaId);

                if (reserva == null)
                    return (false, "Reserva não encontrada.");

                // 2. Validar se pode ser cancelada
                if (reserva.Estado == EstadoReserva.Concluida)
                    return (false, "Não é possível cancelar uma reserva que já foi concluida.");

                if (reserva.Estado == EstadoReserva.Cancelada)
                    return (false, "Esta reserva já foi cancelada.");

                // 3. Mudar estado
                reserva.Estado = EstadoReserva.Cancelada;
                reserva.MotivoCancel = motivo;

                // 4. Mudar veiculo para Ativo (se não tem outra reserva valida)
                var temOutraReserva = await _context.Reservas
                    .AnyAsync(r => r.VeiculoId == reserva.VeiculoId 
                        && r.Id != reserva.Id
                        && r.Valida);

                if (!temOutraReserva)
                {
                    reserva.Veiculo.Estado = EstadoVeiculo.Ativo;
                }

                // 5. Guardar
                _context.Reservas.Update(reserva);
                _context.Veiculos.Update(reserva.Veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva cancelada: Id={ReservaId}, Motivo={Motivo}", reservaId, motivo);
                return (true, "Reserva cancelada com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar reserva: Id={ReservaId}", reservaId);
                return (false, "Erro ao cancelar reserva.");
            }
        }

        /// <summary>
        /// Verificar se um veículo está disponivel para reserva.
        /// </summary>
        public async Task<bool> VeiculoEstaDisponivelAsync(int veiculoId)
        {
            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == veiculoId);
            if (veiculo == null) return false;

            // Disponível se estiver Ativo ou Reservado
            return veiculo.Estado == EstadoVeiculo.Ativo || veiculo.Estado == EstadoVeiculo.Reservado;
        }

        /// <summary>
        /// Limpar reservas que expiraram.
        /// Muda seu estado para "Expirada" e liberta o veículo.
        /// Este método deve ser chamado por um Background Job.
        /// </summary>
        public async Task LimparReservasExpirasAsync()
        {
            try
            {
                var agora = DateTime.UtcNow;

                // 1. Encontrar todas as reservas pendentes que expiraram
                var reservasExpiradas = await _context.Reservas
                    .Include(r => r.Veiculo)
                    .Where(r => r.Estado == EstadoReserva.Pendente && r.DataExpiracao < agora)
                    .ToListAsync();

                _logger.LogInformation("Encontradas {Count} reservas expiradas para limpeza", reservasExpiradas.Count);

                foreach (var reserva in reservasExpiradas)
                {
                    // 2. Mudar estado
                    reserva.Estado = EstadoReserva.Expirada;

                    // 3. Mudar veículo para Ativo
                    reserva.Veiculo.Estado = EstadoVeiculo.Ativo;

                    // 4. Cancelar visitas relacionadas
                    var visitas = await _context.Visitas
                        .Where(v => v.VeiculoId == reserva.VeiculoId 
                            && v.CompradorId == reserva.CompradorId
                            && v.Estado == EstadoVisita.Pendente)
                        .ToListAsync();

                    foreach (var visita in visitas)
                    {
                        visita.Estado = EstadoVisita.Cancelada;
                        visita.MotivoCancel = "Reserva expirou automaticamente";
                    }

                    _context.Visitas.UpdateRange(visitas);
                }

                // 5. Guardar tudo
                _context.Reservas.UpdateRange(reservasExpiradas);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Limpeza de reservas expiradas concluida. Total: {Count}", reservasExpiradas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar reservas expiradas");
            }
        }

        /// <summary>
        /// Confirmar uma compra a partir de uma reserva.
        /// Cria uma Transacao e muda a reserva para "Concluida".
        /// </summary>
        public async Task<(bool sucesso, Transacao? transacao, string mensagem)> ConfirmarCompraAsync(
            int reservaId, 
            int compradorId)
        {
            try
            {
                // 1. Encontrar reserva
                var reserva = await _context.Reservas
                    .Include(r => r.Veiculo)
                    .Include(r => r.Comprador)
                    .FirstOrDefaultAsync(r => r.Id == reservaId && r.CompradorId == compradorId);

                if (reserva == null)
                    return (false, null, "Reserva não encontrada.");

                // 2. Validar se ainda é valida
                if (!reserva.Valida)
                    return (false, null, "Esta reserva não é mais valida. Está expirada ou cancelada.");

                // 3. Validar se o comprador existe
                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.Id == compradorId);
                if (comprador == null)
                    return (false, null, "Comprador não encontrado.");

                // 4. Criar Transacao
                var transacao = new Transacao
                {
                    VeiculoId = reserva.VeiculoId,
                    CompradorId = compradorId,
                    ValorPago = reserva.Veiculo.Preco,
                    Metodo = MetodoPagamento.CartaoCredito,  // Será confirmado no checkout
                    Estado = EstadoTransacao.Pendente,
                    DataTransacao = DateTime.UtcNow
                };

                // 5. Mudar estados
                reserva.Estado = EstadoReserva.Concluida;
                reserva.Veiculo.Estado = EstadoVeiculo.Vendido;

                // 6. Guardar
                _context.Transacoes.Add(transacao);
                _context.Reservas.Update(reserva);
                _context.Veiculos.Update(reserva.Veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Compra confirmada: ReservaId={ReservaId}, TransacaoId={TransacaoId}, Valor={Valor}",
                    reservaId, transacao.Id, transacao.ValorPago);

                return (true, transacao, "Compra confirmada com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao confirmar compra: ReservaId={ReservaId}", reservaId);
                return (false, null, "Erro ao confirmar compra.");
            }
        }

        /// <summary>
        /// Obter todas as reservas de um comprador.
        /// </summary>
        public async Task<List<Reserva>> ObterReservasCompradorAsync(int compradorId)
        {
            return await _context.Reservas
                .Include(r => r.Veiculo)
                .ThenInclude(v => v.Imagens)
                .Include(r => r.Veiculo.Vendedor)
                .ThenInclude(vend => vend.User)
                .Where(r => r.CompradorId == compradorId)
                .OrderByDescending(r => r.DataCriacao)
                .ToListAsync();
        }

        /// <summary>
        /// Obter uma reserva espec�fica.
        /// </summary>
        public async Task<Reserva?> ObterReservaAsync(int reservaId)
        {
            return await _context.Reservas
                .Include(r => r.Veiculo)
                .ThenInclude(v => v.Imagens)
                .Include(r => r.Veiculo.Vendedor)
                .ThenInclude(vend => vend.User)
                .Include(r => r.Comprador)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == reservaId);
        }
    }
}
