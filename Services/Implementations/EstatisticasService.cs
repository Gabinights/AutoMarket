using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de estatísticas.
    /// </summary>
    public class EstatisticasService : IEstatisticasService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<EstatisticasService> _logger;

        public EstatisticasService(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            ILogger<EstatisticasService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<EstatisticasDashboardDto> ObterEstatisticasAsync()
        {
            try
            {
                var stats = new EstatisticasDashboardDto();

                // Contar compradores
                stats.TotalCompradores = await _context.Compradores
                    .Where(c => !c.User!.IsDeleted && !c.User!.IsBlocked)
                    .CountAsync();

                // Contar vendedores aprovados
                stats.TotalVendedores = await _context.Vendedores
                    .Where(v => v.Status == StatusAprovacao.Aprovado && !v.User!.IsDeleted && !v.User!.IsBlocked)
                    .CountAsync();

                // Contar vendedores pendentes
                stats.TotalVendedoresPendentes = await _context.Vendedores
                    .Where(v => v.Status == StatusAprovacao.Pendente)
                    .CountAsync();

                // Contar anúncios ativos
                stats.TotalAnunciosAtivos = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo)
                    .CountAsync();

                // Contar anúncios vendidos
                stats.TotalAnunciosVendidos = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Vendido)
                    .CountAsync();

                // Total de vendas (valor)
                stats.TotalVendasValor = await _context.Transacoes
                    .Where(t => t.Estado == EstadoTransacao.Pago)
                    .SumAsync(t => t.ValorPago);

                // Contar reservas ativas
                stats.TotalReservas = await _context.Reservas
                    .Where(r => r.Estado == EstadoReserva.Confirmada)
                    .CountAsync();

                // Contar visitas confirmadas
                stats.TotalVisitas = await _context.Visitas
                    .Where(v => v.Estado == EstadoVisita.Confirmada)
                    .CountAsync();

                // Vendas por mês (últimos 12 meses)
                var hoje = DateTime.UtcNow;
                stats.VendasPorMes = await _context.Transacoes
                    .Where(t => t.Estado == EstadoTransacao.Pago && 
                                t.DataTransacao >= hoje.AddMonths(-12))
                    .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                    .Select(g => new
                    {
                        Mes = $"{g.Key.Month:00}/{g.Key.Year}",
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Mes)
                    .ToDictionaryAsync(x => x.Mes, x => x.Count);

                // Top marcas
                stats.MarcasPopulares = await _context.Veiculos
                    .Where(v => v.Estado == EstadoVeiculo.Ativo)
                    .GroupBy(v => v.Marca)
                    .Select(g => new { Marca = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToDictionaryAsync(x => x.Marca, x => x.Count);

                // Denúncias por estado
                stats.DenunciasPorEstado = await _context.Denuncias
                    .GroupBy(d => d.Estado)
                    .Select(g => new { Estado = g.Key.ToString(), Count = g.Count() })
                    .ToDictionaryAsync(x => x.Estado, x => x.Count);

                _logger.LogInformation("Estatísticas dashboard obtidas com sucesso");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas dashboard");
                return new EstatisticasDashboardDto();
            }
        }
    }
}
