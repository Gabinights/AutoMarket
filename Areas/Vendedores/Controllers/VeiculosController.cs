using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using AutoMarket.Models.ViewModels.Veiculos;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Vendedores.Controllers
{
    /// <summary>
    /// Controller para gerenciar ve�culos da �rea de Vendedores.
    /// Requer autentica��o e email confirmado.
    /// </summary>
    [Area("Vendedores")]
    [Authorize]
    [Authorize(Policy = "VendedorAprovado")]
    [Route("Vendedores/{controller=Veiculos}/{action=Index}/{id?}")]
    public class VeiculosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<VeiculosController> _logger;

        public VeiculosController(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<VeiculosController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obt�m o vendedor logado pelo userId do contexto.
        /// </summary>
        private async Task<Vendedor?> GetVendedorLogado()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.Vendedores.FirstOrDefaultAsync(v => v.UserId == userId);
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Index
        /// Lista todos os ve�culos do vendedor logado (excluindo removidos).
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

            var veiculos = await _context.Veiculos
                .Where(v => v.VendedorId == vendedor.Id && v.Estado != EstadoVeiculo.Pausado)
                .OrderByDescending(v => v.DataCriacao)
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .ToListAsync();

            var viewModel = veiculos.Select(v => ListVeiculoViewModel.FromVeiculo(v)).ToList();
            return View(viewModel);
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Create
        /// Exibe formul�rio para criar novo ve�culo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateVeiculoViewModel
            {
                CategoriesDropdown = await _context.Categorias.ToListAsync()
            };
            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Veiculos/Create
        /// Cria um novo ve�culo no sistema com upload de imagens.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVeiculoViewModel viewModel)
        {
            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de cria��o de ve�culo sem vendedor logado");
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
                List<VeiculoImagem> imagens = new();

                if (viewModel.Fotos?.Any() == true)
                {
                    var uploadedFiles = await _fileService.UploadMultipleFilesAsync(
                        viewModel.Fotos.ToList(),
                        "images/veiculos"
                    );

                    bool isPrimeiraImagem = true;
                    foreach (var fileName in uploadedFiles)
                    {
                        imagens.Add(new VeiculoImagem
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
                var veiculo = viewModel.ToVeiculo(vendedor.Id);
                veiculo.Imagens = imagens;

                // Guardar na base de dados
                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Ve�culo criado com sucesso. ID: {VeiculoId}, Vendedor: {VendedorId}, Imagens: {ImagemCount}",
                    veiculo.Id, vendedor.Id, imagens.Count);

                TempData["Sucesso"] = $"Ve�culo '{veiculo.Marca} {veiculo.Modelo}' criado com sucesso com {imagens.Count} imagens!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro na valida��o de ficheiro durante cria��o de ve�culo");
                ModelState.AddModelError("Fotos", ex.Message);
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Check for SQL Server Unique Constraint Violation
                if (ex.InnerException is SqlException sqlEx 
                    && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    ModelState.AddModelError("", "Já existe um veículo registado com estes dados.");
                }
                else
                {
                    _logger.LogError(ex, "Database error creating vehicle for vendedor {VendedorId}", vendedor.Id);
                    ModelState.AddModelError("", "Erro de base de dados. Tente novamente.");
                }
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar ve�culo para vendedor {VendedorId}", vendedor.Id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar o ve�culo. Tente novamente.");
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Edit/5
        /// Exibe formul�rio para editar um ve�culo existente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de edi��o sem vendedor logado");
                return Unauthorized();
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
            {
                _logger.LogWarning("Ve�culo n�o encontrado. ID: {VeiculoId}", id);
                return NotFound();
            }

            // Verificar se o ve�culo pertence ao vendedor logado
            if (veiculo.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de edi��o de ve�culo de outro vendedor. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}",
                    id, vendedor.Id);
                return Forbid();
            }

            var viewModel = EditVeiculoViewModel.FromVeiculo(veiculo);
            viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();

            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Veiculos/Edit/5
        /// Atualiza um ve�culo existente com novo upload de imagens (opcional).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditVeiculoViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de edi��o sem vendedor logado");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }

            var veiculoExistente = await _context.Veiculos
                .Include(v => v.Imagens)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculoExistente == null)
            {
                _logger.LogWarning("Ve�culo n�o encontrado para edi��o. ID: {VeiculoId}", id);
                return NotFound();
            }

            if (veiculoExistente.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de edi��o n�o autorizada. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}",
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
                        "images/veiculos"
                    );

                    foreach (var fileName in uploadedFiles)
                    {
                        veiculoExistente.Imagens.Add(new VeiculoImagem
                        {
                            CaminhoFicheiro = fileName,
                            IsCapa = veiculoExistente.Imagens.Count == 0,
                            ContentType = MimeTypeHelper.GetMimeType(fileName),
                            VeiculoId = veiculoExistente.Id
                        });
                    }
                }

                // Atualizar campos do ve�culo
                veiculoExistente.Titulo = viewModel.Titulo;
                veiculoExistente.Marca = viewModel.Marca;
                veiculoExistente.Modelo = viewModel.Modelo;
                veiculoExistente.Ano = viewModel.Ano;
                veiculoExistente.CategoriaId = viewModel.CategoriaId;
                veiculoExistente.Combustivel = viewModel.Combustivel;
                veiculoExistente.Caixa = viewModel.Caixa;
                veiculoExistente.Km = viewModel.Km;
                veiculoExistente.Preco = viewModel.Preco;
                veiculoExistente.Condicao = viewModel.Condicao;
                veiculoExistente.Localizacao = viewModel.Localizacao;
                veiculoExistente.Descricao = viewModel.Descricao;

                _context.Veiculos.Update(veiculoExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Ve�culo atualizado com sucesso. ID: {VeiculoId}, Vendedor: {VendedorId}",
                    id, vendedor.Id);

                TempData["Sucesso"] = "Ve�culo atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro na valida��o de ficheiro durante edi��o de ve�culo");
                ModelState.AddModelError("Fotos", ex.Message);
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Check for SQL Server Unique Constraint Violation
                if (ex.InnerException is SqlException sqlEx 
                    && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    ModelState.AddModelError("", "Já existe um veículo registado com estes dados.");
                }
                else
                {
                    _logger.LogError(ex, "Database error updating vehicle. ID: {VeiculoId}", id);
                    ModelState.AddModelError("", "Erro de base de dados. Tente novamente.");
                }
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar ve�culo. ID: {VeiculoId}", id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar o ve�culo. Tente novamente.");
                viewModel.CategoriesDropdown = await _context.Categorias.ToListAsync();
                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Delete/5
        /// Exibe confirma��o antes de eliminar um ve�culo (soft delete).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de elimina��o sem vendedor logado");
                return Unauthorized();
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
            {
                _logger.LogWarning("Ve�culo n�o encontrado para elimina��o. ID: {VeiculoId}", id);
                return NotFound();
            }

            if (veiculo.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de elimina��o n�o autorizada. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}",
                    id, vendedor.Id);
                return Forbid();
            }

            var viewModel = ListVeiculoViewModel.FromVeiculo(veiculo);
            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Veiculos/Delete/5
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

            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
            {
                _logger.LogWarning("Ve�culo n�o encontrado para soft delete. ID: {VeiculoId}", id);
                return NotFound();
            }

            if (veiculo.VendedorId != vendedor.Id)
            {
                _logger.LogWarning(
                    "Tentativa de soft delete n�o autorizada. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}",
                    id, vendedor.Id);
                return Forbid();
            }

            try
            {
                // Soft Delete: Mudar estado para Pausado (mant�m dados para recuperar)
                veiculo.Estado = EstadoVeiculo.Pausado;

                _context.Veiculos.Update(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Ve�culo removido (soft delete). ID: {VeiculoId}, Vendedor: {VendedorId}",
                    id, vendedor.Id);

                TempData["Sucesso"] = "Ve�culo removido com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover ve�culo. ID: {VeiculoId}", id);
                TempData["Erro"] = "Erro ao remover o ve�culo. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Details/5
        /// Exibe detalhes de um ve�culo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null)
            {
                _logger.LogWarning("Tentativa de visualiza��o de detalhes sem vendedor logado");
                return Unauthorized();
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id && v.VendedorId == vendedor.Id);

            if (veiculo == null)
            {
                _logger.LogWarning("Ve�culo n�o encontrado. ID: {VeiculoId}", id);
                return NotFound();
            }

            var viewModel = ListVeiculoViewModel.FromVeiculo(veiculo);
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
