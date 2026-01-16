using AutoMarket.Models.ViewModels;

namespace AutoMarket.Services.Interfaces
{
    public interface ITransacaoService
    {
        Task<List<TransacaoListViewModel>> GetMinhasComprasAsync(int compradorId);
        Task<List<TransacaoListViewModel>> GetMinhasVendasAsync(int vendedorId);
        Task<(List<Transacao> Transacoes, int TotalCount)> GetHistoricoTransacoesAsync(int page, int pageSize);
    }
}