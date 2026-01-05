using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de estatísticas admin.
    /// </summary>
    public interface IEstatisticasService
    {
        /// <summary>
        /// Obter estatísticas gerais do dashboard.
        /// </summary>
        Task<EstatisticasDashboardDto> ObterEstatisticasAsync();
    }

    /// <summary>
    /// DTO com as estatísticas do dashboard.
    /// </summary>
    public class EstatisticasDashboardDto
    {
        public int TotalCompradores { get; set; }
        public int TotalVendedores { get; set; }
        public int TotalVendedoresPendentes { get; set; }
        public int TotalAnunciosAtivos { get; set; }
        public int TotalAnunciosVendidos { get; set; }
        public decimal TotalVendasValor { get; set; }
        public int TotalReservas { get; set; }
        public int TotalVisitas { get; set; }
        public Dictionary<string, int> VendasPorMes { get; set; } = new();
        public Dictionary<string, int> MarcasPopulares { get; set; } = new();
        public Dictionary<string, int> DenunciasPorEstado { get; set; } = new();
    }
}
