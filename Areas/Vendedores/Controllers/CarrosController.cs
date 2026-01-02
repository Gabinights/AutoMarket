using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using AutoMarket.Models.ViewModels.Carros;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Vendedores.Controllers
{
    /// <summary>
    /// Controller para gerenciar carros da área de Vendedores.
    /// Requer autenticação e email confirmado.
    /// </summary>
    [Area("Vendedores")]
    [Authorize]
    [Authorize(Policy = "VendedorAprovado")]
    [Route("Vendedores/{controller=Carros}/{action=Index}/{id?}")]
    public class CarrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<CarrosController> _logger;

        public CarrosController(
            ApplicationDbContext context, 
            IFileService fileService,
            ILogger<CarrosController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o vendedor logado pelo userId do contexto.
        /// </summary>
        private async Task<Vendedor?> GetVendedorLogado()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.Vendedores.FirstOrDefaultAsync(v => v.UserId == userId);
        }

        /// <summary>
        /// GET: Vendedores/Carros/Index
        /// Lista todos os carros do vendedor logado (excluindo removidos).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de acesso a Index sem vendedor logado");
                return Unauthorized();
            }

            var carros = await _context.Carros
                .Where(c => c.VendedorId == vendedor.Id && c.Estado != EstadoCarro.Pausado)
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();

            var viewModel = carros.Select(c => ListCarroViewModel.FromCarro(c)).ToList();
            return View(viewModel);
        }

        /// <summary>
        /// GET: Vendedores/Carros/Create
        /// Exibe formulário para criar novo carro.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateCarroViewModel
            {
                CategoriesDropdown = await _context.Categorias.ToListAsync()
            };
            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Carros/Create
        /// Cria um novo carro no sistema com upload de imagens.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCarroViewModel viewModel)
        {
            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de criação de carro sem vendedor logado");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }

            try
            {
                // Fazer upload das imagens se fornecidas
                List<CarroImagem> imagens = new();
                
                if (viewModel.Fotos?.Any() == true)
                {
                    var uploadedFiles = await _fileService.UploadMultipleFilesAsync(
                        viewModel.Fotos.ToList(), 
                        "images/cars"
                    );

                    bool isPrimeiraImagem = true;
                    foreach (var fileName in uploadedFiles)
                    {
                        imagens.Add(new CarroImagem
                        {
                            CaminhoFicheiro = fileName,
                            IsCapa = isPrimeiraImagem,
                            ContentType = MimeTypeHelper.GetMimeType(fileName)
                        });

                        if (isPrimeiraImagem)
                            isPrimeiraImagem = false;
                    }
                }

                // Converter ViewModel para Model
                var carro = viewModel.ToCarro(vendedor.Id);
                carro.Imagens = imagens;

                // Guardar na base de dados
                _context.Carros.Add(carro);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Carro criado com sucesso. ID: {CarroId}, Vendedor: {VendedorId}, Imagens: {ImagemCount}", 
                    carro.Id, vendedor.Id, imagens.Count);

                TempData["Sucesso"] = $"Carro '{carro.Marca} {carro.Modelo}' criado com sucesso com {imagens.Count} imagens!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro na validação de ficheiro durante criação de carro");
                ModelState.AddModelError("Fotos", ex.Message);
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar carro para vendedor {VendedorId}", vendedor.Id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar o carro. Tente novamente.");
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: Vendedores/Carros/Edit/5
        /// Exibe formulário para editar um carro existente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de edição sem vendedor logado");
                return Unauthorized();
            }

            var carro = await _context.Carros
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carro == null)
            {
                _logger.LogWarning("Carro não encontrado. ID: {CarroId}", id);
                return NotFound();
            }

            // Verificar se o carro pertence ao vendedor logado
            if (carro.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de edição de carro de outro vendedor. CarroId: {CarroId}, VendedorId: {VendedorId}", 
                    id, vendedor.Id);
                return Forbid();
            }

            var viewModel = EditCarroViewModel.FromCarro(carro);
            viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();

            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Carros/Edit/5
        /// Atualiza um carro existente com novo upload de imagens (opcional).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCarroViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de edição sem vendedor logado");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }

            var carroExistente = await _context.Carros
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carroExistente == null)
            {
                _logger.LogWarning("Carro não encontrado para edição. ID: {CarroId}", id);
                return NotFound();
            }

            if (carroExistente.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de edição não autorizada. CarroId: {CarroId}, VendedorId: {VendedorId}", 
                    id, vendedor.Id);
                return Forbid();
            }

            try
            {
                // Fazer upload de novas imagens se fornecidas
                if (viewModel.Fotos?.Any() == true)
                {
                    var uploadedFiles = await _fileService.UploadMultipleFilesAsync(
                        viewModel.Fotos.ToList(), 
                        "images/cars"
                    );

                    foreach (var fileName in uploadedFiles)
                    {
                        carroExistente.Imagens.Add(new CarroImagem
                        {
                            CaminhoFicheiro = fileName,
                            IsCapa = carroExistente.Imagens.Count == 0, // Primeira imagem é capa
                            ContentType = MimeTypeHelper.GetMimeType(fileName),
                            CarroId = carroExistente.Id
                        });
                    }
                }

                // Atualizar campos do carro
                carroExistente.Titulo = viewModel.Titulo;
                carroExistente.Marca = viewModel.Marca;
                carroExistente.Modelo = viewModel.Modelo;
                carroExistente.Ano = viewModel.Ano;
                carroExistente.CategoriaId = viewModel.CategoriaId;
                carroExistente.Combustivel = viewModel.Combustivel;
                carroExistente.Caixa = viewModel.Caixa;
                carroExistente.Km = viewModel.Km;
                carroExistente.Preco = viewModel.Preco;
                carroExistente.Localizacao = viewModel.Localizacao;
                carroExistente.Descricao = viewModel.Descricao;

                _context.Carros.Update(carroExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Carro atualizado com sucesso. ID: {CarroId}, Vendedor: {VendedorId}", 
                    id, vendedor.Id);

                TempData["Sucesso"] = "Carro atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro na validação de ficheiro durante edição de carro");
                ModelState.AddModelError("Fotos", ex.Message);
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar carro. ID: {CarroId}", id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar o carro. Tente novamente.");
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: Vendedores/Carros/Delete/5
        /// Exibe confirmação antes de eliminar um carro (soft delete).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de eliminação sem vendedor logado");
                return Unauthorized();
            }

            var carro = await _context.Carros
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carro == null)
            {
                _logger.LogWarning("Carro não encontrado para eliminação. ID: {CarroId}", id);
                return NotFound();
            }

            if (carro.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de eliminação não autorizada. CarroId: {CarroId}, VendedorId: {VendedorId}", 
                    id, vendedor.Id);
                return Forbid();
            }

            var viewModel = ListCarroViewModel.FromCarro(carro);
            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Carros/Delete/5
        /// Executa soft delete (muda estado para Pausado).
        /// </summary>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de soft delete sem vendedor logado");
                return Unauthorized();
            }

            var carro = await _context.Carros.FirstOrDefaultAsync(c => c.Id == id);

            if (carro == null)
            {
                _logger.LogWarning("Carro não encontrado para soft delete. ID: {CarroId}", id);
                return NotFound();
            }

            if (carro.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de soft delete não autorizada. CarroId: {CarroId}, VendedorId: {VendedorId}", 
                    id, vendedor.Id);
                return Forbid();
            }

            try
            {
                // Soft Delete: Mudar estado para Pausado (mantém dados para recuperar)
                carro.Estado = EstadoCarro.Pausado;

                _context.Carros.Update(carro);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Carro removido (soft delete). ID: {CarroId}, Vendedor: {VendedorId}", 
                    id, vendedor.Id);

                TempData["Sucesso"] = "Carro removido com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover carro. ID: {CarroId}", id);
                TempData["Erro"] = "Erro ao remover o carro. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: Vendedores/Carros/Details/5
        /// Exibe detalhes de um carro.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de visualização de detalhes sem vendedor logado");
                return Unauthorized();
            }

            var carro = await _context.Carros
                .Include(c => c.Imagens)
                .Include(c => c.Categoria)
                .FirstOrDefaultAsync(c => c.Id == id && c.VendedorId == vendedor.Id);

            if (carro == null)
            {
                _logger.LogWarning("Carro não encontrado. ID: {CarroId}", id);
                return NotFound();
            }

            var viewModel = ListCarroViewModel.FromCarro(carro);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Helper para obter MIME types de ficheiros.
    /// </summary>
    internal static class MimeTypeHelper
    {
        public static string GetMimeType(string fileName)
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
