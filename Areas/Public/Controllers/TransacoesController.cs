using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    public class TransacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<TransacoesController> _logger;

        public TransacoesController(ApplicationDbContext context, UserManager<Utilizador> userManager, ILogger<TransacoesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// GET: Transacoes/Checkout/5
        /// Exibe a página de checkout para compra de veículo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Challenge(); }

            if (string.IsNullOrEmpty(user.NIF))
            {
                TempData["ReturnUrl"] = Url.Action(nameof(Checkout), new { id });
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
