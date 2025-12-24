using AutoMarket.Constants;
using AutoMarket.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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