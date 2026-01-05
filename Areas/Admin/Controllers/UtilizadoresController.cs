using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Admin.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Area("Admin")]
    public class UtilizadoresController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IGestaoUtilizadoresService _gestaoService;
        private readonly ILogger<UtilizadoresController> _logger;

        public UtilizadoresController(
            UserManager<Utilizador> userManager,
            IGestaoUtilizadoresService gestaoService,
            ILogger<UtilizadoresController> logger)
        {
            _userManager = userManager;
            _gestaoService = gestaoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Admin/Utilizadores/Index
        /// Lista utilizadores bloqueados.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var bloqueados = await _gestaoService.ListarUtilizadoresBloqueadosAsync(page, 20);
            var totalBloqueados = await _userManager.Users
                .CountAsync(u => u.IsBlocked && !u.IsDeleted);

            ViewData["TotalPages"] = (int)Math.Ceiling(totalBloqueados / 20.0);
            ViewData["CurrentPage"] = page;

            return View(bloqueados);
        }

        /// <summary>
        /// POST: Admin/Utilizadores/Bloquear
        /// Bloqueia um utilizador.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bloquear(string utilizadorId, string motivo)
        {
            if (string.IsNullOrWhiteSpace(utilizadorId) || string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Erro"] = "Utilizador e motivo são obrigatórios.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var sucesso = await _gestaoService.BloquearUtilizadorAsync(utilizadorId, motivo, adminId);
            if (!sucesso)
            {
                TempData["Erro"] = "Não foi possível bloquear o utilizador.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Sucesso"] = "Utilizador bloqueado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: Admin/Utilizadores/Desbloquear
        /// Desbloqueia um utilizador.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desbloquear(string utilizadorId)
        {
            if (string.IsNullOrWhiteSpace(utilizadorId))
            {
                TempData["Erro"] = "ID do utilizador inválido.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var sucesso = await _gestaoService.DesbloquearUtilizadorAsync(utilizadorId, adminId);
            if (!sucesso)
            {
                TempData["Erro"] = "Não foi possível desbloquear o utilizador.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Sucesso"] = "Utilizador desbloqueado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
    }
}
