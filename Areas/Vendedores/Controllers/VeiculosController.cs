using System.Linq.Expressions;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Models.ViewModels.Veiculos;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Vendedores.Controllers
{
    /// <summary>
    /// Controller para gerenciar veículos da área de Vendedores.
    /// Requer autenticação e email confirmado.
    /// </summary>
    [Area("Vendedores")]
    [Authorize(Policy = "VendedorAprovado")]
    public class VeiculosController : Controller
    {
        private readonly IVeiculoService _veiculoService;
        private readonly IProfileService _profileService;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IFileService _fileService;
        private readonly ILogger<VeiculosController> _logger;

        public VeiculosController(
            IVeiculoService veiculoService,
            IProfileService profileService,
            UserManager<Utilizador> userManager,
            IFileService fileService,
            ILogger<VeiculosController> logger)
        {
            _veiculoService = veiculoService;
            _profileService = profileService;
            _userManager = userManager;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obtem o vendedor logado pelo userId do contexto.
        /// </summary>
        private async Task<Vendedor?> GetVendedorLogado()
        {
            var userId = _userManager.GetUserId(User);
            return await _profileService.GetVendedorByUserIdAsync(userId);
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Index
        /// Lista todos os veiculos do vendedor logado (excluindo removidos).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vendedor = await GetVendedorLogado();
            if (vendedor == null) return Unauthorized();

            var veiculos = await _veiculoService.GetVeiculosByVendedorAsync(vendedor.Id);
            return View(veiculos.Select(ListVeiculoViewModel.FromVeiculo).ToList());
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Create
        /// Exibe formulario para criar novo veiculo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateVeiculoViewModel
            {
                CategoriesDropdown = await _veiculoService.GetCategoriasAsync()
            };
            return View(model);
        }

        /// <summary>
        /// POST: Vendedores/Veiculos/Create
        /// Cria um novo veiculo no sistema com upload de imagens.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVeiculoViewModel model)
        {
            var vendedor = await GetVendedorLogado();
            if (vendedor == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.CategoriesDropdown = await _veiculoService.GetCategoriasAsync();
                return View(model);
            }

            try
            {
                // Fazer upload das imagens se fornecidas
                List<string> imagePaths = new();

                if (model.Fotos?.Any() == true)
                {
                    imagePaths = await _fileService.UploadMultipleFilesAsync(model.Fotos.ToList(), "images/veiculos");
                }

                var veiculo = await _veiculoService.CreateVeiculoAsync(model, vendedor.Id, imagePaths);

                TempData["Sucesso"] = "Veiculo criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar veiculo");
                ModelState.AddModelError(string.Empty, "Erro ao guardar veiculo. " + ex.Message);
                model.CategoriesDropdown = await _veiculoService.GetCategoriasAsync();
                return View(model);
            }
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Edit/5
        /// Exibe formulario para editar um veiculo existente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null) return Unauthorized();

            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id.Value);
            if (veiculo == null) return NotFound();

            if (veiculo.VendedorId != vendedor.Id) return Forbid();

            var viewModel = EditVeiculoViewModel.FromVeiculo(veiculo);
            viewModel.CategoriesDropdown = await _veiculoService.GetCategoriasAsync();

            return View(viewModel);
        }

        /// <summary>
        /// POST: Vendedores/Veiculos/Edit/5
        /// Atualiza um veiculo existente com novo upload de imagens (opcional).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditVeiculoViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                viewModel.CategoriesDropdown = await _veiculoService.GetCategoriasAsync();
                return View(viewModel);
            }

            try
            {
                List<string> newImagePaths = new();
                if (viewModel.Fotos?.Any() == true)
                {
                    newImagePaths = await _fileService.UploadMultipleFilesAsync(viewModel.Fotos.ToList(), "images/veiculos");
                }

                var success = await _veiculoService.UpdateVeiculoAsync(id, viewModel, vendedor.Id, newImagePaths);
                if (!success) return NotFound();

                TempData["Sucesso"] = "Veiculo atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar veiculo.");
                ModelState.AddModelError(string.Empty, "Erro ao atualizar.");
                viewModel.CategoriesDropdown = await _veiculoService.GetCategoriasAsync();
                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Delete/5
        /// Exibe confirmacao antes de eliminar um veiculo (soft delete).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            var vendedor = await GetVendedorLogado();
            if (vendedor == null) return Unauthorized();

            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id.Value);
            if (veiculo == null) return NotFound();
            if (veiculo.VendedorId != vendedor.Id) return Forbid();

            return View(ListVeiculoViewModel.FromVeiculo(veiculo));
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
            if (vendedor == null) return Unauthorized();

            var success = await _veiculoService.SoftDeleteVeiculoAsync(id, vendedor.Id);
            if (!success)
            {
                TempData["Erro"] = "Erro ao remover veiculo ou permissão negada.";
            }
            else
            {
                TempData["Sucesso"] = "Veiculo removido com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: Vendedores/Veiculos/Details/5
        /// Exibe detalhes de um veiculo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return NotFound();
            var vendedor = await GetVendedorLogado();
            if (vendedor == null) return Unauthorized();

            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id.Value);
            if (veiculo == null) return NotFound();
            if (veiculo.VendedorId != vendedor.Id) return Forbid();

            return View(ListVeiculoViewModel.FromVeiculo(veiculo));
        }
    }
}
