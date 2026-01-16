using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.ViewModels;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comprador?> GetCompradorByUserIdAsync(string userId)
        {
            return await _context.Compradores.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Vendedor?> GetVendedorByUserIdAsync(string userId)
        {
            return await _context.Vendedores
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task<PerfilStatsViewModel> GetUserStatsAsync(string userId)
        {
            var stats = new PerfilStatsViewModel();

            var comprador = await GetCompradorByUserIdAsync(userId);
            if (comprador != null)
            {
                stats.TotalCompras = await _context.Transacoes.CountAsync(t => t.CompradorId == comprador.Id);
                stats.IsComprador = true;
            }

            var vendedor = await GetVendedorByUserIdAsync(userId);
            if (vendedor != null)
            {
                stats.TotalVendas = await _context.Transacoes.CountAsync(t => t.VendedorId == vendedor.Id);
                stats.TotalVeiculos = await _context.Veiculos.CountAsync(v => v.VendedorId == vendedor.Id);
                stats.StatusVendedor = vendedor.Status.ToString();
                stats.IsVendedor = true;
            }

            return stats;
        }
    }
}