using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [Authorize] // Só utilizadores logados compram
    public class TransacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public TransacoesController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transacoes/Checkout/5
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Challenge(); }

            var carro = await _context.Carros
                .Include(c => c.Vendedor)
                .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carro == null) return NotFound();

            if (carro.Estado != AutoMarket.Models.Enums.EstadoCarro.Ativo)
            {
                 TempData["Erro"] = "Este veículo já não está disponível.";
                 return RedirectToAction("Index", "Veiculos");
            }

            if (carro.Vendedor.UserId == null || carro.Vendedor == null)
            {
                _logger.LogError("Carro {CarroId} tem Vendedor ou UserId nulos.", id);
                return NotFound();
            }

            if (carro.Vendedor.UserId == user.Id)
            {
                TempData["Erro"] = "Não pode comprar o seu próprio veículo.";
                return RedirectToAction("Detalhes", "Veiculos", new { id = carro.Id });
            }
            return View(carro);
        }
    }
}
