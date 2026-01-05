using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritosApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IFavoritoService _favoritoService;
        private readonly ILogger<FavoritosApiController> _logger;

        public FavoritosApiController(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            IFavoritoService favoritoService,
            ILogger<FavoritosApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _favoritoService = favoritoService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/favoritos/adicionar/{veiculoId}
        /// Adiciona um veículo aos favoritos.
        /// </summary>
        [HttpPost("adicionar/{veiculoId}")]
        public async Task<IActionResult> Adicionar(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Obter ID do comprador
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
                return BadRequest(new { message = "Utilizador não é comprador registado." });

            var sucesso = await _favoritoService.AdicionarFavoritoAsync(comprador.Id, veiculoId);
            if (!sucesso)
                return BadRequest(new { message = "Não foi possível adicionar o favorito." });

            return Ok(new { message = "Favorito adicionado com sucesso." });
        }

        /// <summary>
        /// DELETE: api/favoritos/remover/{veiculoId}
        /// Remove um veículo dos favoritos.
        /// </summary>
        [HttpDelete("remover/{veiculoId}")]
        public async Task<IActionResult> Remover(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
                return BadRequest(new { message = "Utilizador não é comprador registado." });

            var sucesso = await _favoritoService.RemoverFavoritoAsync(comprador.Id, veiculoId);
            if (!sucesso)
                return BadRequest(new { message = "Não foi possível remover o favorito." });

            return Ok(new { message = "Favorito removido com sucesso." });
        }

        /// <summary>
        /// GET: api/favoritos/verificar/{veiculoId}
        /// Verifica se um veículo está nos favoritos.
        /// </summary>
        [HttpGet("verificar/{veiculoId}")]
        public async Task<IActionResult> Verificar(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
                return Ok(new { estaNosFavoritos = false });

            var estaNosFavoritos = await _favoritoService.EstaNosFavoritosAsync(comprador.Id, veiculoId);
            return Ok(new { estaNosFavoritos });
        }

        /// <summary>
        /// GET: api/favoritos/listar
        /// Lista os favoritos do utilizador.
        /// </summary>
        [HttpGet("listar")]
        public async Task<IActionResult> Listar(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador == null)
                return BadRequest(new { message = "Utilizador não é comprador registado." });

            var favoritos = await _favoritoService.ListarFavoritosAsync(comprador.Id, page, 20);
            var total = await _favoritoService.ContarFavoritosAsync(comprador.Id);

            return Ok(new { favoritos, total, page });
        }
    }
}
