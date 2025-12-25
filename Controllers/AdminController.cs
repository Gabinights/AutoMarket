using AutoMarket.Constants;
using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Index (Lista de Pendentes)
        public async Task<IActionResult> Index()
        {
            // Busca apenas os vendedores pendentes e inclui os dados do Utilizador (Nome, Email)
            var vendedoresPendentes = await _context.Vendedores
                .Include(v => v.User)
                .Where(v => v.Status == StatusAprovacao.Pendente)
                .OrderBy(v => v.User.DataRegisto)
                .ToListAsync();

            return View(vendedoresPendentes);
        }

        // POST: Admin/Aprovar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(int id)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();

            // Lógica de Domínio
            vendedor.Status = StatusAprovacao.Aprovado;
            vendedor.ApprovedByAdminId = _userManager.GetUserId(User); // Regista quem aprovou
            vendedor.MotivoRejeicao = null; // Limpa rejeições antigas se houver

            await _context.SaveChangesAsync();

            TempData["MensagemStatus"] = "Vendedor aprovado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Rejeitar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();

            // Lógica de Domínio
            vendedor.Status = StatusAprovacao.Rejeitado;
            vendedor.ApprovedByAdminId = _userManager.GetUserId(User); // Regista quem rejeitou
            vendedor.MotivoRejeicao = "Documentação insuficiente ou dados incorretos."; // Podes melhorar isto para receber um input de texto

            await _context.SaveChangesAsync();

            TempData["MensagemStatus"] = "Vendedor rejeitado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/HistoricoTransacoes
        public async Task<IActionResult> HistoricoTransacoes()
        {
            // O .IgnoreQueryFilters() é CRÍTICO aqui.
            // Sem ele, se o Comprador ou Vendedor tiverem IsDeleted=true,
            // o EF Core devolve 'null' nas propriedades de navegação ou exclui a linha (dependendo do join).
            var transacoes = await _context.Transacoes
                .IgnoreQueryFilters()
                .Include(t => t.Carro)
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.User) // Vai buscar o User mesmo se IsDeleted = true
                .OrderByDescending(t => t.DataTransacao)
                .ToListAsync();

            return View(transacoes);
        }
    }
}