using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para o service de gestão de visitas.
    /// </summary>
    public interface IVisitaService
    {
        /// <summary>Agendar uma visita a um veículo.</summary>
        Task<(bool sucesso, Visita? visita, string mensagem)> AgendarVisitaAsync(
            int veiculoId,
            int compradorId,
            DateTime dataHora,
            string? notas = "");

        /// <summary>Cancelar uma visita agendada.</summary>
        Task<(bool sucesso, string mensagem)> CancelarVisitaAsync(
            int visitaId,
            string motivo = "");

        /// <summary>Confirmar uma visita (ação do vendedor).</summary>
        Task<(bool sucesso, string mensagem)> ConfirmarVisitaAsync(int visitaId);

        /// <summary>Marcar visita como realizada (ação do vendedor).</summary>
        Task<(bool sucesso, string mensagem)> MarcarComoRealizadaAsync(
            int visitaId,
            string? notas = "");

        /// <summary>Validar data/hora da visita.</summary>
        bool ValidarDataVisita(DateTime dataHora);

        /// <summary>Validar se veículo foi vendido.</summary>
        Task<bool> ValidarVeiculoVendidoAsync(int veiculoId);

        /// <summary>Obter todas as visitas de um comprador.</summary>
        Task<List<Visita>> ObterVisitasCompradorAsync(int compradorId);

        /// <summary>Obter todas as visitas de um vendedor.</summary>
        Task<List<Visita>> ObterVisitasVendedorAsync(int vendedorId);

        /// <summary>Obter uma visita específica.</summary>
        Task<Visita?> ObterVisitaAsync(int visitaId);
    }
}
