using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [Authorize]
    public class TransacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public TransacoesController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// GET: Transacoes/Checkout/5
        /// Exibe a página de checkout para compra de veículo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrEmpty(user.NIF))
            {
                TempData["ReturnUrl"] = Url.Action(nameof(Checkout), new { id = id });
                return RedirectToAction("PreencherDadosFiscais", "Conta");
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.Vendedor)
                .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
                return NotFound();

            return View(veiculo);
        }
    }
}
