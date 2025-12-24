using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Vendedores.Controllers
{
    /// <summary>
    /// Controller para gerenciar veículos da área de Vendedores.
    /// Requer autenticação e email confirmado.
    /// </summary>
    [Area("Vendedores")]
    [Authorize]
    [Route("Vendedores/{controller=Carros}/{action=Index}/{id?}")]
    public class CarrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CarrosController> _logger;

        public CarrosController(ApplicationDbContext context, ILogger<CarrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: Vendedores/Carros/Index
        /// Lista todos os veículos do vendedor logado (excluindo removidos).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(vendedorId))
            {
                _logger.LogWarning("Tentativa de acesso a Index sem vendedorId");
                return Unauthorized();
            }

            var veiculos = await _context.Veiculos
                .Where(v => v.VendedorId == vendedorId && v.Estado != EstadoVeiculo.Removido)
                .OrderByDescending(v => v.DataCriacao)
                .ToListAsync();

            return View(veiculos);
        }

        /// <summary>
        /// GET: Vendedores/Carros/Create
        /// Exibe formulário para criar novo veículo.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Vendedores/Carros/Create
        /// Cria um novo veículo no sistema.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Veiculo veiculo)
        {
            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendedorId))
            {
                _logger.LogWarning("Tentativa de criação de veículo sem vendedorId");
                return Unauthorized();
            }

            // Atribuir vendedor logado
            veiculo.VendedorId = vendedorId;
            veiculo.DataCriacao = DateTime.UtcNow;
            veiculo.Estado = EstadoVeiculo.Ativo;

            // Validação customizada
            if (string.IsNullOrWhiteSpace(veiculo.Matricula))
                ModelState.AddModelError(nameof(veiculo.Matricula), "A matrícula é obrigatória.");

            if (string.IsNullOrWhiteSpace(veiculo.Marca))
                ModelState.AddModelError(nameof(veiculo.Marca), "A marca é obrigatória.");

            if (string.IsNullOrWhiteSpace(veiculo.Modelo))
                ModelState.AddModelError(nameof(veiculo.Modelo), "O modelo é obrigatório.");

            if (veiculo.Preco <= 0)
                ModelState.AddModelError(nameof(veiculo.Preco), "O preço deve ser superior a 0.");

            // Verificar duplicação de matrícula
            var matriculaExiste = await _context.Veiculos
                .AnyAsync(v => v.Matricula == veiculo.Matricula && v.Id != veiculo.Id);

            if (matriculaExiste)
                ModelState.AddModelError(nameof(veiculo.Matricula), "Já existe um veículo com esta matrícula.");

            if (!ModelState.IsValid)
                return View(veiculo);

            try
            {
                _context.Add(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Veículo criado com sucesso. ID: {VeiculoId}, Vendedor: {VendedorId}", veiculo.Id, vendedorId);
                TempData["Sucesso"] = $"Veículo '{veiculo.Marca} {veiculo.Modelo}' criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao criar veículo para vendedor {VendedorId}", vendedorId);
                ModelState.AddModelError(string.Empty, "Erro ao guardar o veículo. Tente novamente.");
                return View(veiculo);
            }
        }

        /// <summary>
        /// GET: Vendedores/Carros/Edit/5
        /// Exibe formulário para editar um veículo existente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendedorId))
                return Unauthorized();

            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
            {
                _logger.LogWarning("Veículo não encontrado. ID: {VeiculoId}", id);
                return NotFound();
            }

            // Verificar se o veículo pertence ao vendedor logado
            if (veiculo.VendedorId != vendedorId)
            {
                _logger.LogWarning("Tentativa de edição de veículo de outro vendedor. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}", id, vendedorId);
                return Forbid();
            }

            // Não permitir edição de veículos removidos
            if (veiculo.Estado == EstadoVeiculo.Removido)
            {
                TempData["Erro"] = "Não é possível editar um veículo removido.";
                return RedirectToAction(nameof(Index));
            }

            return View(veiculo);
        }

        /// <summary>
        /// POST: Vendedores/Carros/Edit/5
        /// Atualiza um veículo existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Veiculo veiculo)
        {
            if (id != veiculo.Id)
                return NotFound();

            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendedorId))
                return Unauthorized();

            var veiculoExistente = await _context.Veiculos.FindAsync(id);

            if (veiculoExistente == null)
            {
                _logger.LogWarning("Veículo não encontrado para edição. ID: {VeiculoId}", id);
                return NotFound();
            }

            if (veiculoExistente.VendedorId != vendedorId)
            {
                _logger.LogWarning("Tentativa de edição não autorizada. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}", id, vendedorId);
                return Forbid();
            }

            if (veiculoExistente.Estado == EstadoVeiculo.Removido)
            {
                TempData["Erro"] = "Não é possível editar um veículo removido.";
                return RedirectToAction(nameof(Index));
            }

            // Validação
            if (string.IsNullOrWhiteSpace(veiculo.Matricula))
                ModelState.AddModelError(nameof(veiculo.Matricula), "A matrícula é obrigatória.");

            if (veiculo.Preco <= 0)
                ModelState.AddModelError(nameof(veiculo.Preco), "O preço deve ser superior a 0.");

            var matriculaExiste = await _context.Veiculos
                .AnyAsync(v => v.Matricula == veiculo.Matricula && v.Id != veiculo.Id);

            if (matriculaExiste)
                ModelState.AddModelError(nameof(veiculo.Matricula), "Já existe um veículo com esta matrícula.");

            if (!ModelState.IsValid)
                return View(veiculo);

            try
            {
                // Atualizar apenas campos permitidos
                veiculoExistente.Matricula = veiculo.Matricula;
                veiculoExistente.Marca = veiculo.Marca;
                veiculoExistente.Modelo = veiculo.Modelo;
                veiculoExistente.Versao = veiculo.Versao;
                veiculoExistente.Cor = veiculo.Cor;
                veiculoExistente.Categoria = veiculo.Categoria;
                veiculoExistente.Ano = veiculo.Ano;
                veiculoExistente.Quilometros = veiculo.Quilometros;
                veiculoExistente.Combustivel = veiculo.Combustivel;
                veiculoExistente.Caixa = veiculo.Caixa;
                veiculoExistente.Potencia = veiculo.Potencia;
                veiculoExistente.Portas = veiculo.Portas;
                veiculoExistente.Preco = veiculo.Preco;
                veiculoExistente.Condicao = veiculo.Condicao;
                veiculoExistente.Descricao = veiculo.Descricao;
                veiculoExistente.ImagemPrincipal = veiculo.ImagemPrincipal;
                veiculoExistente.DataModificacao = DateTime.UtcNow;

                _context.Update(veiculoExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Veículo atualizado com sucesso. ID: {VeiculoId}, Vendedor: {VendedorId}", id, vendedorId);
                TempData["Sucesso"] = "Veículo atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar veículo. ID: {VeiculoId}", id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar o veículo. Tente novamente.");
                return View(veiculo);
            }
        }

        /// <summary>
        /// GET: Vendedores/Carros/Delete/5
        /// Exibe confirmação antes de eliminar um veículo (soft delete).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendedorId))
                return Unauthorized();

            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
            {
                _logger.LogWarning("Veículo não encontrado para eliminação. ID: {VeiculoId}", id);
                return NotFound();
            }

            if (veiculo.VendedorId != vendedorId)
            {
                _logger.LogWarning("Tentativa de eliminação não autorizada. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}", id, vendedorId);
                return Forbid();
            }

            return View(veiculo);
        }

        /// <summary>
        /// POST: Vendedores/Carros/Delete/5
        /// Executa soft delete (muda estado para Removido e guarda data).
        /// </summary>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendedorId))
                return Unauthorized();

            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
            {
                _logger.LogWarning("Veículo não encontrado para soft delete. ID: {VeiculoId}", id);
                return NotFound();
            }

            if (veiculo.VendedorId != vendedorId)
            {
                _logger.LogWarning("Tentativa de soft delete não autorizada. VeiculoId: {VeiculoId}, VendedorId: {VendedorId}", id, vendedorId);
                return Forbid();
            }

            try
            {
                // Soft Delete: Mudar estado para Removido
                veiculo.Estado = EstadoVeiculo.Removido;
                veiculo.DataRemocao = DateTime.UtcNow;

                _context.Update(veiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Veículo removido (soft delete). ID: {VeiculoId}, Vendedor: {VendedorId}", id, vendedorId);
                TempData["Sucesso"] = "Veículo removido com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao remover veículo. ID: {VeiculoId}", id);
                TempData["Erro"] = "Erro ao remover o veículo. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: Vendedores/Carros/Details/5
        /// Exibe detalhes de um veículo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var vendedorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendedorId))
                return Unauthorized();

            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null || veiculo.VendedorId != vendedorId)
                return NotFound();

            return View(veiculo);
        }
    }
}
