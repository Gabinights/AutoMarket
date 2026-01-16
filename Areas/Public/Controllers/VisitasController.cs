using AutoMarket.Services.Interfaces;
using AutoMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize]
    public class VisitasController : Controller
    {
        private readonly IVisitaService _visitaService;
        private readonly IVeiculoService _veiculoService;
        private readonly IProfileService _profileService;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<VisitasController> _logger;

        public VisitasController(
            IVisitaService visitaService,
            IVeiculoService veiculoService,
            IProfileService profileService,
            UserManager<Utilizador> userManager,
            ILogger<VisitasController> logger)
        {
            _visitaService = visitaService;
            _veiculoService = veiculoService;
            _profileService = profileService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Agendar(int veiculoId)
        {
            var veiculo = await _veiculoService.GetVeiculoEntityAsync(veiculoId);
            if (veiculo == null) return NotFound();

            if (await _visitaService.ValidarVeiculoVendidoAsync(veiculoId))
            {
                TempData["Erro"] = "Este veículo já foi vendido.";
                return RedirectToAction("Detalhe", "Veiculos", new { id = veiculoId });
            }

            ViewBag.VeiculoId = veiculoId;
            ViewBag.DataMinima = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
            return View(veiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(int veiculoId, DateTime dataHora, string? notas)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);
            if (comprador == null)
            {
                TempData["Erro"] = "Perfil de comprador não encontrado.";
                return RedirectToAction("Index", "Veiculos");
            }

            var (sucesso, visita, mensagem) = await _visitaService.AgendarVisitaAsync(veiculoId, comprador.Id, dataHora, notas);

            if (!sucesso)
            {
                TempData["Erro"] = mensagem;
                return RedirectToAction("Agendar", new { veiculoId });
            }

            TempData["Sucesso"] = mensagem;
            return RedirectToAction("MinhasVisitas");
        }

        [HttpGet]
        public async Task<IActionResult> MinhasVisitas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);
            if (comprador == null) return RedirectToAction("Index", "Veiculos");

            var visitas = await _visitaService.ObterVisitasCompradorAsync(comprador.Id);
            return View(visitas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarVisita(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var visita = await _visitaService.ObterVisitaAsync(id);
            if (visita == null) return RedirectToAction("MinhasVisitas");

            if (visita.Comprador.UserId != user.Id) return Forbid();

            var (sucesso, mensagem) = await _visitaService.CancelarVisitaAsync(id, "Cancelado pelo comprador");
            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("MinhasVisitas");
        }

        [HttpGet]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> VendedorDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendedor = await _profileService.GetVendedorByUserIdAsync(user.Id);
            if (vendedor == null) return RedirectToAction("Index", "Home");

            var visitas = await _visitaService.ObterVisitasVendedorAsync(vendedor.Id);
            return View(visitas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> ConfirmarVisita(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var visita = await _visitaService.ObterVisitaAsync(id);
            if (visita == null || visita.Vendedor.UserId != user.Id) return Forbid();

            var (sucesso, mensagem) = await _visitaService.ConfirmarVisitaAsync(id);
            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("VendedorDashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> MarcarRealizada(int id, string? notas)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var visita = await _visitaService.ObterVisitaAsync(id);
            if (visita == null || visita.Vendedor.UserId != user.Id) return Forbid();

            var (sucesso, mensagem) = await _visitaService.MarcarComoRealizadaAsync(id, notas);
            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("VendedorDashboard");
        }
    }
}