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
        public async Task<IActionResult> Checkout(int id) // id do Carro
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Challenge(); } // Nunca deve acontecer devido ao [Authorize]

            // === LÓGICA DE VALIDAÇÃO ===
            if (string.IsNullOrEmpty(user.NIF))
            {
                // Guardamos a URL atual completa para voltar exatamente aqui
                TempData["ReturnUrl"] = Url.Action(nameof(Checkout), new { id = id });

                return RedirectToAction("PreencherDadosFiscais", "Conta");
            }
            // ================================

            // Se tem NIF, prossegue com a busca do carro para mostrar o resumo
            var carro = await _context.Carros
                .Include(c => c.Vendedor)
                .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carro == null) return NotFound();

            // Aqui passarias um 'TransacaoViewModel' ou o próprio Carro para a View
            return View(carro);
        }
    }
}
