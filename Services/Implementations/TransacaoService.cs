using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.ViewModels;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ApplicationDbContext _context;

        public TransacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Transacao> Transacoes, int TotalCount)> GetHistoricoTransacoesAsync(int page, int pageSize)
        {
            var query = _context.Transacoes
                .IgnoreQueryFilters()
                .Include(t => t.Veiculo)
                    .ThenInclude(c => c.Vendedor)
                        .ThenInclude(v => v.User)
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.User)
                .OrderByDescending(t => t.DataTransacao);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }
        public async Task<List<TransacaoListViewModel>> GetMinhasComprasAsync(int compradorId)
        {
            return await _context.Transacoes
                .Include(t => t.Veiculo)
                    .ThenInclude(v => v.Imagens)
                .Include(t => t.Veiculo)
                    .ThenInclude(v => v.Vendedor)
                        .ThenInclude(vend => vend.User)
                .Where(t => t.CompradorId == compradorId)
                .OrderByDescending(t => t.DataTransacao)
                .Select(t => new TransacaoListViewModel
                {
                    TransacaoId = t.Id,
                    DataTransacao = t.DataTransacao,
                    ValorPago = t.ValorPago,
                    Estado = t.Estado,
                    Metodo = t.Metodo,
                    VeiculoId = t.VeiculoId,
                    VeiculoTitulo = t.Veiculo.Titulo,
                    VeiculoMarca = t.Veiculo.Marca,
                    VeiculoModelo = t.Veiculo.Modelo,
                    // Get cover image or first image, null if none
                    VeiculoImagemCapa = t.Veiculo.Imagens
                        .Where(i => i.IsCapa)
                        .Select(i => i.CaminhoFicheiro)
                        .FirstOrDefault()
                        ?? t.Veiculo.Imagens
                        .Select(i => i.CaminhoFicheiro)
                        .FirstOrDefault(),
                    VendedorNome = t.Veiculo.Vendedor.User.Nome,
                    MoradaEnvioSnapshot = t.MoradaEnvioSnapshot,
                    NifFaturacaoSnapshot = t.NifFaturacaoSnapshot
                })
                .ToListAsync();
        }
        public async Task<List<TransacaoListViewModel>> GetMinhasVendasAsync(int vendedorId)
        {
            return await _context.Transacoes
                .Include(t => t.Veiculo)
                    .ThenInclude(v => v.Imagens)
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.User)
                .Where(t => t.VendedorId == vendedorId)
                .OrderByDescending(t => t.DataTransacao)
                .Select(t => new TransacaoListViewModel
                {
                    TransacaoId = t.Id,
                    DataTransacao = t.DataTransacao,
                    ValorPago = t.ValorPago,
                    Estado = t.Estado,
                    Metodo = t.Metodo,
                    VeiculoId = t.VeiculoId,
                    VeiculoTitulo = t.Veiculo.Titulo,
                    VeiculoMarca = t.Veiculo.Marca,
                    VeiculoModelo = t.Veiculo.Modelo,
                    VeiculoImagemCapa = t.Veiculo.Imagens
                        .Where(i => i.IsCapa)
                        .Select(i => i.CaminhoFicheiro)
                        .FirstOrDefault()
                        ?? t.Veiculo.Imagens
                        .Select(i => i.CaminhoFicheiro)
                        .FirstOrDefault(),
                    CompradorNome = t.Comprador.User.Nome,
                    MoradaEnvioSnapshot = t.MoradaEnvioSnapshot,
                    NifFaturacaoSnapshot = t.NifFaturacaoSnapshot
                })
                .ToListAsync();
        }
    }
}