using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models.ViewModels.Veiculos;

namespace AutoMarket.Services.Implementations
{
    public class VeiculoService : IVeiculoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VeiculoService> _logger;

        public VeiculoService(ApplicationDbContext context, ILogger<VeiculoService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Veiculo?> GetVeiculoEntityAsync(int id)
        {
            return await _context.Veiculos
            .Include(v => v.Imagens)
            .Include(v => v.Categoria)
            .Include(v => v.Vendedor)
            .ThenInclude(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<VeiculoDto?> GetVeiculoByIdAsync(int id)
        {
            var v = await GetVeiculoEntityAsync(id);
            if (v == null) return null;
            bool IsDeleted = v.Estado == EstadoVeiculo.Pausado;
            return new VeiculoDto(v.Id, v.Marca, v.Modelo, v.Ano, v.Preco, v.Estado, IsDeleted);
        }

        public async Task<List<Veiculo>> GetVeiculosByVendedorAsync(int vendedorId)
        {
            return await _context.Veiculos
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .Where(v => v.VendedorId == vendedorId && v.Estado != EstadoVeiculo.Pausado)
                .OrderByDescending(v => v.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task<Veiculo> CreateVeiculoAsync(CreateVeiculoViewModel model, int vendedorId, List<string> imagePaths)
        {
            // Map ViewModel to Entity
            var veiculo = model.ToVeiculo(vendedorId);

            // Add Images
            bool isCapa = true;
            foreach (var path in imagePaths)
            {
                veiculo.Imagens.Add(new VeiculoImagem
                {
                    CaminhoFicheiro = path,
                    ContentType = GetMimeType(path),
                    IsCapa = isCapa
                });
                isCapa = false;
            }

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();
            return veiculo;
        }

        public async Task<bool> UpdateVeiculoAsync(int id, EditVeiculoViewModel model, int vendedorId, List<string> newImagePaths)
        {
            var veiculo = await _context.Veiculos.Include(v => v.Imagens).FirstOrDefaultAsync(v => v.Id == id);
            if (veiculo == null) return false;
            if (veiculo.VendedorId != vendedorId) throw new UnauthorizedAccessException("Veículo não pertence ao vendedor.");

            // Update Fields
            veiculo.Titulo = model.Titulo;
            veiculo.Marca = model.Marca;
            veiculo.Modelo = model.Modelo;
            veiculo.Ano = model.Ano;
            veiculo.CategoriaId = model.CategoriaId;
            veiculo.Combustivel = model.Combustivel;
            veiculo.Caixa = model.Caixa;
            veiculo.Km = model.Km;
            veiculo.Preco = model.Preco;
            veiculo.Condicao = model.Condicao;
            veiculo.Localizacao = model.Localizacao;
            veiculo.Descricao = model.Descricao;

            // Add new images
            foreach (var path in newImagePaths)
            {
                veiculo.Imagens.Add(new VeiculoImagem
                {
                    CaminhoFicheiro = path,
                    ContentType = "image/jpeg",
                    IsCapa = veiculo.Imagens.Count == 0,
                    VeiculoId = veiculo.Id
                });
            }

            _context.Veiculos.Update(veiculo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteVeiculoAsync(int id, int vendedorId)
        {
            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id);
            if (veiculo == null) return false;
            if (veiculo.VendedorId != vendedorId) return false;

            veiculo.Estado = EstadoVeiculo.Pausado;

            _context.Veiculos.Update(veiculo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Veiculo> Veiculos, int TotalCount, int TotalPages)> SearchVeiculosAsync(VeiculoSearchFiltersDto filters)
        {
            var query = _context.Veiculos
                .Where(v => v.Estado == EstadoVeiculo.Ativo)
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(filters.Marca)) query = query.Where(v => v.Marca == filters.Marca);
            if (!string.IsNullOrEmpty(filters.Modelo)) query = query.Where(v => v.Modelo == filters.Modelo);
            if (!string.IsNullOrEmpty(filters.Combustivel)) query = query.Where(v => v.Combustivel == filters.Combustivel);
            if (filters.Ano.HasValue) query = query.Where(v => v.Ano == filters.Ano);
            if (!string.IsNullOrEmpty(filters.Categoria)) query = query.Where(v => v.Categoria.Nome == filters.Categoria);
            if (filters.PrecoMin.HasValue) query = query.Where(v => v.Preco >= filters.PrecoMin);
            if (filters.PrecoMax.HasValue) query = query.Where(v => v.Preco <= filters.PrecoMax);
            if (filters.KmMin.HasValue) query = query.Where(v => v.Km >= filters.KmMin);
            if (filters.KmMax.HasValue) query = query.Where(v => v.Km <= filters.KmMax);

            // Apply Sorting
            query = filters.Ordenacao switch
            {
                "preco_asc" => query.OrderBy(v => v.Preco),
                "preco_desc" => query.OrderByDescending(v => v.Preco),
                "km_asc" => query.OrderBy(v => v.Km),
                "km_desc" => query.OrderByDescending(v => v.Km),
                "ano_asc" => query.OrderBy(v => v.Ano),
                "ano_desc" => query.OrderByDescending(v => v.Ano),
                _ => query.OrderByDescending(v => v.DataCriacao)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize);

            var items = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return (items, totalCount, totalPages);
        }

        public async Task<VeiculoFilterOptionsDto> GetFilterOptionsAsync()
        {
            var veiculosAtivos = await _context.Veiculos
                .Where(v => v.Estado == EstadoVeiculo.Ativo)
                .Include(v => v.Categoria)
                .ToListAsync();

            return new VeiculoFilterOptionsDto
            {
                Marcas = veiculosAtivos.Select(v => v.Marca).Distinct().OrderBy(m => m).ToList(),
                Modelos = veiculosAtivos.Select(v => v.Modelo).Distinct().OrderBy(m => m).ToList(),
                Combustiveis = veiculosAtivos.Where(v => !string.IsNullOrEmpty(v.Combustivel)).Select(v => v.Combustivel).Distinct().OrderBy(c => c).ToList(),
                Anos = veiculosAtivos.Select(v => v.Ano).Distinct().OrderByDescending(a => a).ToList(),
                Categorias = veiculosAtivos.Where(v => v.Categoria != null).Select(v => v.Categoria.Nome).Distinct().OrderBy(c => c).ToList()
            };
        }

        public async Task<bool> VeiculoBelongsToVendedorAsync(int veiculoId, int vendedorId)
        {
            return await _context.Veiculos.AnyAsync(v => v.Id == veiculoId && v.VendedorId == vendedorId);
        }

        public async Task<bool> IsVeiculoVendidoAsync(int veiculoId)
        {
            var v = await _context.Veiculos.FindAsync(veiculoId);
            return v?.Estado == EstadoVeiculo.Vendido;
        }

        // --- admin methods ---
        public async Task<(List<Veiculo> Veiculos, int TotalCount)> GetVeiculosParaModeracaoAsync(string? estado, int page, int pageSize)
        {
            var query = _context.Veiculos
                .Include(v => v.Vendedor)
                .ThenInclude(v => v.User)
                .Include(v => v.Imagens)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(v => v.Estado.ToString() == estado);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(v => v.DataCriacao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> PausarVeiculoAdminAsync(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return false;

            veiculo.Estado = EstadoVeiculo.Pausado;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Anúncio {VeiculoId} pausado pelo admin", id);
            return true;
        }

        public async Task<bool> RemoverVeiculoAdminAsync(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return false;

            _context.Remove(veiculo); // Hard delete??
            await _context.SaveChangesAsync();

            _logger.LogInformation("Anúncio {VeiculoId} removido pelo admin", id);
            return true;
        }
        private static string GetMimeType(string fileName)
        {
            return Path.GetExtension(fileName).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "image/jpeg"
            };
        }
    }
}