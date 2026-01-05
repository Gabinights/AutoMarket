using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public AdminController(
            ApplicationDbContext context, 
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Admin/Index (Lista de Pendentes)
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var query = _context.Vendedores
                .Include(v => v.User)
                .Where(v => v.Status == StatusAprovacao.Pendente)
                .OrderBy(v => v.User.DataRegisto);

            var model = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);

            return View(model);
        }

        // POST: Admin/Aprovar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(int id)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();

            vendedor.Aprovar(_userManager.GetUserId(User));
            await _context.SaveChangesAsync();

            // Refresh do cookie para atualizar as claims (StatusVendedor)
            var user = await _userManager.FindByIdAsync(vendedor.UserId);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["MensagemStatus"] = "Vendedor aprovado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Rejeitar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id, string motivo)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();

            if (string.IsNullOrWhiteSpace(motivo))
            {
                motivo = "Documentação insuficiente ou dados incorretos.";
            }

            vendedor.Rejeitar(_userManager.GetUserId(User), motivo);
            await _context.SaveChangesAsync();

            // Refresh do cookie para atualizar as claims (StatusVendedor)
            var user = await _userManager.FindByIdAsync(vendedor.UserId);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["MensagemStatus"] = "Vendedor rejeitado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/HistoricoTransacoes
        public async Task<IActionResult> HistoricoTransacoes(int page = 1)
        {
            int pageSize = 20;

            var query = _context.Transacoes
                .IgnoreQueryFilters()
                .Include(t => t.Veiculo)
                    .ThenInclude(c => c.Vendedor)
                        .ThenInclude(v => v.User)
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.User)
                .OrderByDescending(t => t.DataTransacao);

            var model = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);

            return View(model);
        }
    }
}