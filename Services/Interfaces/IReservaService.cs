using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para o service de gest�o de reservas.
    /// </summary>
    public interface IReservaService
    {
        /// <summary>Criar uma nova reserva.</summary>
        Task<(bool sucesso, Reserva? reserva, string mensagem)> CriarReservaAsync(
            int veiculoId, 
            int compradorId, 
            int diasValidez = 7);

        /// <summary>Cancelar uma reserva existente.</summary>
        Task<(bool sucesso, string mensagem)> CancelarReservaAsync(
            int reservaId, 
            string motivo = "");

        /// <summary>Verificar se um veiculo esta disponivel.</summary>
        Task<bool> VeiculoEstaDisponivelAsync(int veiculoId);

        /// <summary>Limpar reservas que expiraram (background job).</summary>
        Task LimparReservasExpirasAsync();

        /// <summary>Confirmar a compra de um veiculo reservado.</summary>
        Task<(bool sucesso, Transacao? transacao, string mensagem)> ConfirmarCompraAsync(
            int reservaId, 
            int compradorId);

        /// <summary>Obter todas as reservas de um comprador.</summary>
        Task<List<Reserva>> ObterReservasCompradorAsync(int compradorId);

        /// <summary>Obter uma reserva espec�fica.</summary>
        Task<Reserva?> ObterReservaAsync(int reservaId);
    }
}
