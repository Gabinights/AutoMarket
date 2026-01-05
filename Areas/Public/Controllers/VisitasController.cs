using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    /// <summary>
    /// Controller para gestão de visitas agendadas.
    /// Apenas para utilizadores autenticados.
    /// </summary>
    [Area("Public")]
    [Authorize]
    public class VisitasController : Controller
    {
        private readonly IVisitaService _visitaService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<VisitasController> _logger;

        public VisitasController(
            IVisitaService visitaService,
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            ILogger<VisitasController> logger)
        {
            _visitaService = visitaService;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Visitas/Agendar/5
        /// Mostrar formulário para agendar visita.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Agendar(int veiculoId)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Imagens)
                .Include(v => v.Vendedor)
                .ThenInclude(vend => vend.User)
                .FirstOrDefaultAsync(v => v.Id == veiculoId);

            if (veiculo == null)
                return NotFound();

            // Verificar se veículo foi vendido
            if (await _visitaService.ValidarVeiculoVendidoAsync(veiculoId))
            {
                TempData["Erro"] = "Este veículo já foi vendido.";
                return RedirectToAction("Detalhe", "Veiculos", new { id = veiculoId });
            }

            ViewBag.VeiculoId = veiculoId;
            ViewBag.DataMinima = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm"); // ISO 8601 para input[type=datetime-local]

            return View(veiculo);
        }

        /// <summary>
        /// POST: /Visitas/Agendar
        /// Submeter agendamento de visita.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(int veiculoId, DateTime dataHora, string? notas)
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

            // Agendar visita
            var (sucesso, visita, mensagem) = await _visitaService.AgendarVisitaAsync(
                veiculoId,
                comprador.Id,
                dataHora,
                notas);

            if (!sucesso)
            {
                TempData["Erro"] = mensagem;
                return RedirectToAction("Agendar", new { veiculoId });
            }

            TempData["Sucesso"] = mensagem;
            return RedirectToAction("MinhasVisitas");
        }

        /// <summary>
        /// GET: /Visitas/MinhasVisitas
        /// Listar todas as visitas do comprador.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MinhasVisitas()
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

            var visitas = await _visitaService.ObterVisitasCompradorAsync(comprador.Id);

            return View(visitas);
        }

        /// <summary>
        /// POST: /Visitas/CancelarVisita/5
        /// Cancelar uma visita agendada.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarVisita(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Validar permissão
            var visita = await _context.Visitas
                .Include(v => v.Comprador)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
            {
                TempData["Erro"] = "Visita não encontrada.";
                return RedirectToAction("MinhasVisitas");
            }

            if (visita.Comprador.UserId != user.Id)
            {
                TempData["Erro"] = "Não tem permissão para cancelar esta visita.";
                return RedirectToAction("MinhasVisitas");
            }

            // Cancelar
            var (sucesso, mensagem) = await _visitaService.CancelarVisitaAsync(
                id,
                motivo: "Cancelado pelo comprador");

            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("MinhasVisitas");
        }

        /// <summary>
        /// GET: /Visitas/VendedorDashboard
        /// Painel do vendedor com visitas agendadas.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> VendedorDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendedor = await _context.Vendedores
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendedor == null)
            {
                TempData["Erro"] = "Perfil de vendedor não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var visitas = await _visitaService.ObterVisitasVendedorAsync(vendedor.Id);

            return View(visitas);
        }

        /// <summary>
        /// POST: /Visitas/ConfirmarVisita/5
        /// Confirmar uma visita agendada (ação do vendedor).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> ConfirmarVisita(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Validar permissão
            var visita = await _context.Visitas
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
            {
                TempData["Erro"] = "Visita não encontrada.";
                return RedirectToAction("VendedorDashboard");
            }

            if (visita.Vendedor.UserId != user.Id)
            {
                TempData["Erro"] = "Não tem permissão.";
                return RedirectToAction("VendedorDashboard");
            }

            // Confirmar
            var (sucesso, mensagem) = await _visitaService.ConfirmarVisitaAsync(id);

            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("VendedorDashboard");
        }

        /// <summary>
        /// POST: /Visitas/MarcarRealizada/5
        /// Marcar visita como realizada (ação do vendedor).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> MarcarRealizada(int id, string? notas)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Validar permissão
            var visita = await _context.Visitas
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null || visita.Vendedor.UserId != user.Id)
            {
                TempData["Erro"] = "Não tem permissão.";
                return RedirectToAction("VendedorDashboard");
            }

            var (sucesso, mensagem) = await _visitaService.MarcarComoRealizadaAsync(id, notas);

            TempData[sucesso ? "Sucesso" : "Erro"] = mensagem;
            return RedirectToAction("VendedorDashboard");
        }
    }
}
