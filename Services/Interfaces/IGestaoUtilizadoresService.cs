using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de gestão de utilizadores (bloqueio, desbloqueio, etc).
    /// </summary>
    public interface IGestaoUtilizadoresService
    {
        /// <summary>
        /// Bloquear um utilizador com motivo.
        /// </summary>
        Task<bool> BloquearUtilizadorAsync(string utilizadorId, string motivo, string adminId);

        /// <summary>
        /// Desbloquear um utilizador.
        /// </summary>
        Task<bool> DesbloquearUtilizadorAsync(string utilizadorId, string adminId);

        /// <summary>
        /// Obter histórico de bloqueios de um utilizador.
        /// </summary>
        Task<string?> ObterMotivoBloquoAsync(string utilizadorId);

        /// <summary>
        /// Listar utilizadores bloqueados.
        /// </summary>
        Task<List<Utilizador>> ListarUtilizadoresBloqueadosAsync(int page = 1, int pageSize = 20);
    }
}
