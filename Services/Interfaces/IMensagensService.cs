using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    public interface IMensagensService
    {
        Task<Mensagem> EnviarMensagemAsync(string remetenteId, string destinatarioId, string conteudo, int? veiculoId = null);
        Task<List<Mensagem>> ListarConversaAsync(string userId1, string userId2, int page = 1, int pageSize = 50);
        Task<bool> MarcarComoLidaAsync(int mensagemId);
        Task<int> ContarNaolidasAsync(string userId);
    }
}
