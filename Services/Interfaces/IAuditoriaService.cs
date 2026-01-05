using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de auditoria.
    /// Regista todas as ações administrativas.
    /// </summary>
    public interface IAuditoriaService
    {
        /// <summary>
        /// Registar uma ação administrativa.
        /// </summary>
        Task RegistarAcaoAsync(
            string adminId,
            string tipoAcao,
            string descricao,
            string? entidadeAfetadaId = null,
            string? tipoEntidade = null,
            string? dadosAntigos = null,
            string? dadosNovos = null,
            string? enderecoIP = null,
            string? userAgent = null);

        /// <summary>
        /// Obter logs de auditoria paginados.
        /// </summary>
        Task<List<AuditoriaLog>> ListarLogsAsync(int page = 1, int pageSize = 50);

        /// <summary>
        /// Obter logs de um admin específico.
        /// </summary>
        Task<List<AuditoriaLog>> ListarLogsPorAdminAsync(string adminId, int page = 1, int pageSize = 50);
    }
}
