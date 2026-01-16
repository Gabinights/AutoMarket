using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    public class VendedorService : IVendedorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VendedorService> _logger;

        public VendedorService(ApplicationDbContext context, ILogger<VendedorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<Vendedor> Vendedores, int TotalCount)> GetVendedoresPendentesAsync(int page, int pageSize)
        {
            var query = _context.Vendedores
                .Include(v => v.User)
                .Where(v => v.Status == StatusAprovacao.Pendente)
                .OrderBy(v => v.User.DataRegisto);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }

        public async Task<Vendedor?> GetVendedorByIdAsync(int id)
        {
            return await _context.Vendedores.Include(v => v.User).FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<string?> AprovarVendedorAsync(int id, string adminId)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null) return null;

            vendedor.Aprovar(adminId);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vendedor {Id} aprovado pelo admin {AdminId}", id, adminId);
            return vendedor.UserId;
        }

        public async Task<string?> RejeitarVendedorAsync(int id, string adminId, string motivo)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null) return null;

            vendedor.Rejeitar(adminId, motivo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vendedor {Id} rejeitado pelo admin {AdminId}. Motivo: {Motivo}", id, adminId, motivo);
            return vendedor.UserId;
        }
    }
}