using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Service para gestao de visitas agendadas a veiculos.
    /// Controla o ciclo de vida: Agendamento, Confirmacao, Realizacao.
    /// </summary>
    public class VisitaService : IVisitaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VisitaService> _logger;

        public VisitaService(ApplicationDbContext context, ILogger<VisitaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Agendar uma visita a um veiculo.
        /// Valida data/hora e estado do veiculo.
        /// </summary>
        public async Task<(bool sucesso, Visita? visita, string mensagem)> AgendarVisitaAsync(
            int veiculoId,
            int compradorId,
            DateTime dataHora,
            string? notas = "")
        {
            try
            {
                // 1. Validar data/hora
                if (!ValidarDataVisita(dataHora))
                    return (false, null, "Data/hora invalida. Deve ser uma data futura durante horario de funcionamento.");

                // 2. Validar veiculo existe
                var veiculo = await _context.Veiculos
                    .Include(v => v.Vendedor)
                    .FirstOrDefaultAsync(v => v.Id == veiculoId);

                if (veiculo == null)
                    return (false, null, "Veiculo nao encontrado.");

                // 3. Validar veiculo nao esta vendido
                if (await ValidarVeiculoVendidoAsync(veiculoId))
                    return (false, null, "Nao e possivel agendar visita. Este veiculo ja foi vendido.");

                // 4. Validar comprador existe
                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.Id == compradorId);

                if (comprador == null)
                    return (false, null, "Perfil de comprador nao encontrado.");

                // 5. Validar se nao tem visita no mesmo horario (opcional - limite de 5 visitas por dia)
                var visitasNesseDia = await _context.Visitas
                    .Where(v => v.VeiculoId == veiculoId
                        && v.DataHora.Date == dataHora.Date
                        && v.Estado != EstadoVisita.Cancelada)
                    .CountAsync();

                if (visitasNesseDia >= 5)
                    return (false, null, "Limite de visitas atingido para este dia. Escolha outra data.");

                // 6. Criar a visita
                var visita = new Visita
                {
                    VeiculoId = veiculoId,
                    CompradorId = compradorId,
                    VendedorId = veiculo.VendedorId,
                    DataHora = dataHora,
                    DataAgendamento = DateTime.UtcNow,
                    Estado = EstadoVisita.Pendente,
                    Notas = notas
                };

                // 7. Guardar
                _context.Visitas.Add(visita);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Visita agendada: VeiculoId={VeiculoId}, CompradorId={CompradorId}, DataHora={DataHora}",
                    veiculoId, compradorId, dataHora);

                return (true, visita, "Visita agendada com sucesso! Aguardando confirma��o do vendedor.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao agendar visita: VeiculoId={VeiculoId}, CompradorId={CompradorId}",
                    veiculoId, compradorId);
                return (false, null, "Erro ao agendar visita.");
            }
        }

        /// <summary>
        /// Cancelar uma visita agendada.
        /// </summary>
        public async Task<(bool sucesso, string mensagem)> CancelarVisitaAsync(
            int visitaId,
            string motivo = "")
        {
            try
            {
                // 1. Encontrar visita
                var visita = await _context.Visitas.FirstOrDefaultAsync(v => v.Id == visitaId);
                if (visita == null)
                    return (false, "Visita n�o encontrada.");

                // 2. Validar se pode ser cancelada
                if (visita.Estado == EstadoVisita.Realizada)
                    return (false, "N�o � poss�vel cancelar uma visita que j� foi realizada.");

                if (visita.Estado == EstadoVisita.Cancelada)
                    return (false, "Esta visita j� foi cancelada.");

                // 3. Mudar estado
                visita.Estado = EstadoVisita.Cancelada;
                visita.MotivoCancel = motivo;

                // 4. Guardar
                _context.Visitas.Update(visita);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Visita cancelada: Id={VisitaId}, Motivo={Motivo}", visitaId, motivo);
                return (true, "Visita cancelada com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar visita: Id={VisitaId}", visitaId);
                return (false, "Erro ao cancelar visita.");
            }
        }

        /// <summary>
        /// Confirmar uma visita agendada (a��o do vendedor).
        /// </summary>
        public async Task<(bool sucesso, string mensagem)> ConfirmarVisitaAsync(int visitaId)
        {
            try
            {
                // 1. Encontrar visita
                var visita = await _context.Visitas.FirstOrDefaultAsync(v => v.Id == visitaId);
                if (visita == null)
                    return (false, "Visita n�o encontrada.");

                // 2. Validar estado
                if (visita.Estado != EstadoVisita.Pendente)
                    return (false, "Apenas visitas pendentes podem ser confirmadas.");

                // 3. Mudar estado
                visita.Estado = EstadoVisita.Confirmada;

                // 4. Guardar
                _context.Visitas.Update(visita);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Visita confirmada: Id={VisitaId}", visitaId);
                return (true, "Visita confirmada com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao confirmar visita: Id={VisitaId}", visitaId);
                return (false, "Erro ao confirmar visita.");
            }
        }

        /// <summary>
        /// Marcar uma visita como realizada (a��o do vendedor).
        /// </summary>
        public async Task<(bool sucesso, string mensagem)> MarcarComoRealizadaAsync(
            int visitaId,
            string? notas = "")
        {
            try
            {
                var visita = await _context.Visitas.FirstOrDefaultAsync(v => v.Id == visitaId);
                if (visita == null)
                    return (false, "Visita nao encontrada.");

                // Apenas visitas no passado podem ser marcadas como realizadas
                if (!visita.DataJaPassou)
                    return (false, "Nao pode marcar como realizada antes da data/hora agendada.");

                visita.Estado = EstadoVisita.Realizada;
                if (!string.IsNullOrEmpty(notas))
                    visita.NotasVendedor = notas;

                _context.Visitas.Update(visita);
                await _context.SaveChangesAsync();

                return (true, "Visita marcada como realizada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar visita como realizada: Id={VisitaId}", visitaId);
                return (false, "Erro ao atualizar visita.");
            }
        }

        /// <summary>
        /// Validar se uma data/hora e valida para agendamento.
        /// - Deve ser no futuro
        /// - Deve estar em horario de funcionamento (9-18h)
        /// </summary>
        public bool ValidarDataVisita(DateTime dataHora)
        {
            var agora = DateTime.UtcNow;

            // 1. Deve ser no futuro (minimo 1 hora a partir de agora)
            if (dataHora <= agora.AddHours(1))
                return false;

            // 2. Horario de funcionamento: 9-18h
            if (dataHora.Hour < 9 || dataHora.Hour >= 18)
                return false;

            // 3. So diasuteis (segunda a sexta)
            if (dataHora.DayOfWeek == DayOfWeek.Saturday || dataHora.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return true;
        }

        /// <summary>
        /// Validar se um veiculo foi vendido.
        /// </summary>
        public async Task<bool> ValidarVeiculoVendidoAsync(int veiculoId)
        {
            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == veiculoId);
            return veiculo?.Estado == EstadoVeiculo.Vendido;
        }

        /// <summary>
        /// Obter todas as visitas de um comprador.
        /// </summary>
        public async Task<List<Visita>> ObterVisitasCompradorAsync(int compradorId)
        {
            return await _context.Visitas
                .Include(v => v.Veiculo)
                .ThenInclude(ve => ve.Imagens)
                .Include(v => v.Vendedor)
                .ThenInclude(vend => vend.User)
                .Where(v => v.CompradorId == compradorId)
                .OrderBy(v => v.DataHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obter todas as visitas de um vendedor.
        /// </summary>
        public async Task<List<Visita>> ObterVisitasVendedorAsync(int vendedorId)
        {
            return await _context.Visitas
                .Include(v => v.Veiculo)
                .ThenInclude(ve => ve.Imagens)
                .Include(v => v.Comprador)
                .ThenInclude(c => c.User)
                .Where(v => v.VendedorId == vendedorId)
                .OrderBy(v => v.DataHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obter uma visita espec�fica.
        /// </summary>
        public async Task<Visita?> ObterVisitaAsync(int visitaId)
        {
            return await _context.Visitas
                .Include(v => v.Veiculo)
                .ThenInclude(ve => ve.Imagens)
                .Include(v => v.Comprador)
                .ThenInclude(c => c.User)
                .Include(v => v.Vendedor)
                .ThenInclude(vend => vend.User)
                .FirstOrDefaultAsync(v => v.Id == visitaId);
        }
    }
}
