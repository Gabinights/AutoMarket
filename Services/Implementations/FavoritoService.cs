using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de favoritos.
    /// </summary>
    public class FavoritoService : IFavoritoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoritoService> _logger;

        public FavoritoService(
            ApplicationDbContext context,
            ILogger<FavoritoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AdicionarFavoritoAsync(int compradorId, int veiculoId)
        {
            try
            {
                var existe = await _context.Favoritos
                    .AnyAsync(f => f.CompradorId == compradorId && f.VeiculoId == veiculoId);

                if (existe)
                {
                    _logger.LogWarning(
                        "Tentativa de adicionar favorito duplicado: Comprador {CompradorId}, Veículo {VeiculoId}",
                        compradorId, veiculoId);
                    return false;
                }

                var favorito = new Favorito
                {
                    CompradorId = compradorId,
                    VeiculoId = veiculoId,
                    DataAdicao = DateTime.UtcNow
                };

                _context.Favoritos.Add(favorito);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Favorito adicionado: Comprador {CompradorId}, Veículo {VeiculoId}",
                    compradorId, veiculoId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar favorito");
                return false;
            }
        }

        public async Task<bool> RemoverFavoritoAsync(int compradorId, int veiculoId)
        {
            try
            {
                var favorito = await _context.Favoritos
                    .FirstOrDefaultAsync(f => f.CompradorId == compradorId && f.VeiculoId == veiculoId);

                if (favorito == null)
                {
                    _logger.LogWarning(
                        "Tentativa de remover favorito inexistente: Comprador {CompradorId}, Veículo {VeiculoId}",
                        compradorId, veiculoId);
                    return false;
                }

                _context.Favoritos.Remove(favorito);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Favorito removido: Comprador {CompradorId}, Veículo {VeiculoId}",
                    compradorId, veiculoId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover favorito");
                return false;
            }
        }

        public async Task<bool> EstaNosFavoritosAsync(int compradorId, int veiculoId)
        {
            return await _context.Favoritos
                .AnyAsync(f => f.CompradorId == compradorId && f.VeiculoId == veiculoId);
        }

        public async Task<List<Veiculo>> ListarFavoritosAsync(int compradorId, int page = 1, int pageSize = 20)
        {
            return await _context.Favoritos
                .Where(f => f.CompradorId == compradorId)
                .Include(f => f.Veiculo)
                    .ThenInclude(v => v.Imagens)
                .Include(f => f.Veiculo.Vendedor)
                    .ThenInclude(vnd => vnd.User)
                .Include(f => f.Veiculo.Categoria)
                .OrderByDescending(f => f.DataAdicao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Veiculo)
                .ToListAsync();
        }

        public async Task<int> ContarFavoritosAsync(int compradorId)
        {
            return await _context.Favoritos
                .CountAsync(f => f.CompradorId == compradorId);
        }
    }
}
