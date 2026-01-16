using AutoMarket.Services.Interfaces;
using AutoMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly IReservaService _reservaService;
        private readonly IVeiculoService _veiculoService;
        private readonly IProfileService _profileService;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(
            IReservaService reservaService,
            IVeiculoService veiculoService,
            IProfileService profileService,
            UserManager<Utilizador> userManager,
            ILogger<ReservasController> logger)
        {
            _reservaService = reservaService;
            _veiculoService = veiculoService;
            _profileService = profileService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Criar(int id)
        {
            var veiculo = await _veiculoService.GetVeiculoEntityAsync(id);
            if (veiculo == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);
            if (comprador == null)
            {
                TempData["Erro"] = "Perfil de comprador não encontrado.";
                return RedirectToAction("Index", "Veiculos");
            }

            return View(veiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarReserva(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);
            if (comprador == null)
            {
                TempData["Erro"] = "Perfil de comprador não encontrado.";
                return RedirectToAction("Index", "Veiculos");
            }

            var (sucesso, reserva, mensagem) = await _reservaService.CriarReservaAsync(veiculoId, comprador.Id, 7);

            if (!sucesso)
            {
                TempData["Erro"] = mensagem;
                return RedirectToAction("Detalhe", "Veiculos", new { id = veiculoId });
            }

            TempData["Sucesso"] = mensagem;
            return RedirectToAction("Minhas");
        }

        [HttpGet]
        public async Task<IActionResult> Minhas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);
            if (comprador == null) return RedirectToAction("Index", "Veiculos");

            var reservas = await _reservaService.ObterReservasCompradorAsync(comprador.Id);
            return View(reservas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reserva = await _reservaService.ObterReservaAsync(id);
            if (reserva == null) return RedirectToAction("Minhas");

            if (reserva.Comprador.UserId != user.Id)
            {
                TempData["Erro"] = "Não tem permissão.";
                return RedirectToAction("Minhas");
            }

            var (sucesso, mensagem) = await _reservaService.CancelarReservaAsync(id, "Cancelado pelo comprador");
            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("Minhas");
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reserva = await _reservaService.ObterReservaAsync(id);
            if (reserva == null) return NotFound();

            if (reserva.Comprador.UserId != user.Id) return Forbid();

            return View(reserva);
        }
    }
}