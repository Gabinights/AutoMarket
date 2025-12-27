using AutoMarket.Constants;
using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models.ViewModels;

namespace AutoMarket.Controllers
{
    [Authorize(Roles = Roles.Vendedor)]
    [Authorize(Policy = "VendedorAprovado")]
    public class VeiculosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment; // Para upload de imagens

        public VeiculosController(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // Método Auxiliar para obter o Vendedor Logado
        private async Task<Vendedor?> GetVendedorLogadoAsync()
        {
            var userId = _userManager.GetUserId(User);
            // Inclui o User para podermos verificar outras coisas se necessário
            return await _context.Vendedores
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }

        // GET: Veiculos
        public async Task<IActionResult> Index()
        {
            var vendedor = await GetVendedorLogadoAsync();
            if (vendedor == null) return View("Error"); // Ou redirecionar para configurar perfil

            // Mostra APENAS os carros deste vendedor
            var carros = await _context.Carros
                .Include(c => c.Categoria)
                .Include(c => c.Imagens) // Para mostrar a foto de capa na lista
                .Where(c => c.VendedorId == vendedor.Id)
                .ToListAsync();

            return View(carros);
        }

        // GET: Veiculos/Create
        public IActionResult Create()
        {
            // Verifica se o vendedor está aprovado antes de deixar criar
            // (Lógica opcional baseada no teu StatusAprovacao)

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome");
            return View(new CarroViewModel());
        }

        // POST: Veiculos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarroViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", viewModel.CategoriaId);
                return View(viewModel);
            }

            var vendedor = await GetVendedorLogadoAsync();

            if (vendedor == null) return RedirectToAction("Register", "Conta");


            // -- Mapear (ViewModel -> Domain Model) --- 
            var carro = new Carro
            {
                Titulo = viewModel.Titulo,
                Marca = viewModel.Marca,
                Modelo = viewModel.Modelo,
                CategoriaId = viewModel.CategoriaId,
                Ano = viewModel.Ano,
                Preco = viewModel.Preco,
                Km = viewModel.Km,
                Combustivel = viewModel.Combustivel,
                Caixa = viewModel.Caixa,
                Localizacao = viewModel.Localizacao,
                Descricao = viewModel.Descricao,

                // Campos de sistema (automáticos)
                VendedorId = vendedor.Id,
                DataCriacao = DateTime.UtcNow,
                Estado = EstadoCarro.Ativo
            };

            // Lógica de Imagens
            if (viewModel.ImagensUpload != null)
            {
                foreach (var img in viewModel.ImagensUpload)
                {
                    if (img.Length > 0)
                    {
                        // Validação básica
                        if (!img.ContentType.StartsWith("image/")) continue; 
                        if (img.Length > 5 * 1024 * 1024) continue; // Max 5MB
                        var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/carros");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        
                        var path = Path.Combine(uploadsFolder, uniqueName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await img.CopyToAsync(stream);
                        }

                        carro.Imagens.Add(new CarroImagem
                        {
                            CaminhoFicheiro = uniqueName,
                            ContentType = img.ContentType,
                            IsCapa = carro.Imagens.Count == 0
                        });
                    }
                }
            }

            _context.Add(carro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: Veiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vendedor = await GetVendedorLogadoAsync();
            if (vendedor == null) return Forbid();

            // Verificar se o carro existe E SE PERTENCE AO VENDEDOR LOGADO
            var carro = await _context.Carros
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c => c.Id == id && c.VendedorId == vendedor.Id);
            if (carro == null) { return NotFound(); }

            // --- Mapeamento Inverso (Domain -> ViewModel) ---
            // Preencher formulário com os dados da BD
            var viewModel = new CarroViewModel
            {
                Id = carro.Id,
                Titulo = carro.Titulo,
                Marca = carro.Marca,
                Modelo = carro.Modelo,
                CategoriaId = carro.CategoriaId,
                Ano = carro.Ano,
                Preco = carro.Preco,
                Km = carro.Km,
                Combustivel = carro.Combustivel,
                Caixa = carro.Caixa,
                Localizacao = carro.Localizacao,
                Descricao = carro.Descricao,
                ImagensAtuais = carro.Imagens
            };

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", carro.CategoriaId);
            return View(viewModel);
        }

        // POST: Veiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarroViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", viewModel.CategoriaId);
                return View(viewModel);
            }

            var vendedor = await GetVendedorLogadoAsync();
            if (vendedor == null) return Forbid();

            var carroOriginal = await _context.Carros
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c => c.Id == id && c.VendedorId == vendedor.Id);

            if (carroOriginal == null) return NotFound();

            // --- Mapeamento de atualização (ViewModel -> Domain) ---
            // Apenas atualizamos os campos permitidos
            carroOriginal.Titulo = viewModel.Titulo;
            carroOriginal.Marca = viewModel.Marca;
            carroOriginal.Modelo = viewModel.Modelo;
            carroOriginal.CategoriaId = viewModel.CategoriaId;
            carroOriginal.Ano = viewModel.Ano;
            carroOriginal.Preco = viewModel.Preco;
            carroOriginal.Km = viewModel.Km;
            carroOriginal.Combustivel = viewModel.Combustivel;
            carroOriginal.Caixa = viewModel.Caixa;
            carroOriginal.Localizacao = viewModel.Localizacao;
            carroOriginal.Descricao = viewModel.Descricao;

            // Nota: não mexer em VendedorId, DataCriacao ou Estado aqui

            // Lógica de adicionar novas imagens (se houver)
            if (viewModel.ImagensUpload != null)
            {
                foreach (var img in viewModel.ImagensUpload)
                {
                    if (img.Length > 0)
                    {
                        // Validação básica
                        if (!img.ContentType.StartsWith("image/")) continue; 
                        if (img.Length > 5 * 1024 * 1024) continue; // Max 5MB

                        var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/carros");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        var path = Path.Combine(uploadsFolder, uniqueName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await img.CopyToAsync(stream);
                        }

                        carroOriginal.Imagens.Add(new CarroImagem
                        {
                            CaminhoFicheiro = uniqueName,
                            ContentType = img.ContentType,
                            IsCapa = carroOriginal.Imagens.Count == 0
                        });
                    }
                }
            }

            try
            {
                _context.Update(carroOriginal);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Carros.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Veiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendedor = await GetVendedorLogadoAsync();
            if (vendedor == null) return Forbid();

            // SEGURANÇA: Filtrar também pelo VendedorId no Delete
            var carro = await _context.Carros
                .FirstOrDefaultAsync(c => c.Id == id && c.VendedorId == vendedor.Id);

            if (carro != null)
            {
                // Opcional: Apagar as imagens físicas do disco antes de apagar o registo
                _context.Carros.Remove(carro);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}