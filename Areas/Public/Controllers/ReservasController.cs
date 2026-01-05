using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    /// <summary>
    /// Controller para gestão de reservas de veículos.
    /// Apenas para utilizadores autenticados.
    /// </summary>
    [Area("Public")]
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly IReservaService _reservaService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(
            IReservaService reservaService,
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            ILogger<ReservasController> logger)
        {
            _reservaService = reservaService;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Reservas/Criar/5
        /// Mostrar detalhes do veículo e botão para confirmar reserva.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Criar(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .Include(v => v.Categoria)
                .Include(v => v.Vendedor)
                .ThenInclude(vend => vend.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
                return NotFound();

            // Verificar se comprador existe
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
            {
                TempData["Erro"] = "Perfil de comprador não encontrado.";
                return RedirectToAction("Index", "Veiculos");
            }

            return View(veiculo);
        }

        /// <summary>
        /// POST: /Reservas/Criar
        /// Submeter a reserva.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarReserva(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
            {
                TempData["Erro"] = "Perfil de comprador não encontrado.";
                return RedirectToAction("Index", "Veiculos");
            }

            // Criar reserva
            var (sucesso, reserva, mensagem) = await _reservaService.CriarReservaAsync(
                veiculoId,
                comprador.Id,
                diasValidez: 7);

            if (!sucesso)
            {
                TempData["Erro"] = mensagem;
                return RedirectToAction("Detalhe", "Veiculos", new { id = veiculoId });
            }

            TempData["Sucesso"] = mensagem;
            return RedirectToAction("Minhas");
        }

        /// <summary>
        /// GET: /Reservas/Minhas
        /// Listar todas as reservas do comprador autenticado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Minhas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
            {
                TempData["Erro"] = "Perfil de comprador não encontrado.";
                return RedirectToAction("Index", "Veiculos");
            }

            var reservas = await _reservaService.ObterReservasCompradorAsync(comprador.Id);

            return View(reservas);
        }

        /// <summary>
        /// POST: /Reservas/Cancelar/5
        /// Cancelar uma reserva.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Validar que a reserva pertence ao comprador
            var reserva = await _context.Reservas
                .Include(r => r.Comprador)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                TempData["Erro"] = "Reserva não encontrada.";
                return RedirectToAction("Minhas");
            }

            if (reserva.Comprador.UserId != user.Id)
            {
                TempData["Erro"] = "Não tem permissão para cancelar esta reserva.";
                return RedirectToAction("Minhas");
            }

            var (sucesso, mensagem) = await _reservaService.CancelarReservaAsync(
                id,
                motivo: "Cancelado pelo comprador");

            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("Minhas");
        }

        /// <summary>
        /// GET: /Reservas/Detalhes/5
        /// Ver detalhes de uma reserva específica.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reserva = await _reservaService.ObterReservaAsync(id);

            if (reserva == null)
                return NotFound();

            // Verificar permissão
            if (reserva.Comprador.UserId != user.Id)
            {
                return Forbid();
            }

            return View(reserva);
        }
    }
}
