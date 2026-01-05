using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<Transacao> ProcessCheckoutAsync(string userId, Transacao checkout);
        Task<Transacao?> GetTransacaoAsync(string userId, int transacaoId);
    }
}