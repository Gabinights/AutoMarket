namespace AutoMarket.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<CheckoutInitDto> GetCheckoutAsync(string userId);
        Task<CheckoutProcessResultDto> ProcessAsync(string userId, CheckoutInputDto input);
        Task<TransacaoDto?> GetTransacaoAsync(string userId, int transacaoId);
    }
}