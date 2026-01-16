using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de notificações (in-app + email).
    /// </summary>
    public interface INotificacaoService
    {
        /// <summary>
        /// Criar notificação para um utilizador específico.
        /// </summary>
        Task NotificarUtilizadorAsync(
            string utilizadorId,
            string tipo,
            string assunto,
            string corpo,
            string? linkRelacionado = null,
            string? entidadeRelacionadaId = null,
            string? tipoEntidadeRelacionada = null);

        /// <summary>
        /// Notificar todos os admins.
        /// </summary>
        Task NotificarAdminsAsync(
            string tipo,
            string assunto,
            string corpo,
            string? linkRelacionado = null,
            string? entidadeRelacionadaId = null,
            string? tipoEntidadeRelacionada = null);

        /// <summary>
        /// Marcar notificação como lida.
        /// </summary>
        Task<bool> MarcarComolida(int notificacaoId);

        /// <summary>
        /// Obter notificações nao lidas de um utilizador.  
        /// </summary>
        Task<List<Notificacao>> ObterNaolidasAsync(string utilizadorId);

        /// <summary>
        /// Obter todas as notificações de um utilizador com paginação.
        /// </summary>
        Task<List<Notificacao>> ListarNotificacoesAsync(string utilizadorId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Obter o número total de notificações de um utilizador.
        /// </summary>
        Task<int> ContarNotificacoesAsync(string utilizadorId);
    }
}
