using AutoMarket.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritosApiController : ControllerBase
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IProfileService _profileService;
        private readonly IFavoritoService _favoritoService;
        private readonly ILogger<FavoritosApiController> _logger;

        public FavoritosApiController(
            UserManager<Utilizador> userManager,
            IProfileService profileService,
            IFavoritoService favoritoService,
            ILogger<FavoritosApiController> logger)
        {
            _userManager = userManager;
            _profileService = profileService;
            _favoritoService = favoritoService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/favoritos/adicionar/{veiculoId}
        /// Adiciona um veiculo aos favoritos.
        /// </summary>
        [HttpPost("adicionar/{veiculoId}")]
        public async Task<IActionResult> Adicionar(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Obter ID do comprador
            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);

            if (comprador == null)
                return BadRequest(new { message = "Utilizador n�o � comprador registado." });

            var sucesso = await _favoritoService.AdicionarFavoritoAsync(comprador.Id, veiculoId);
            if (!sucesso)
                return BadRequest(new { message = "N�o foi poss�vel adicionar o favorito." });

            return Ok(new { message = "Favorito adicionado com sucesso." });
        }

        /// <summary>
        /// DELETE: api/favoritos/remover/{veiculoId}
        /// Remove um ve�culo dos favoritos.
        /// </summary>
        [HttpDelete("remover/{veiculoId}")]
        public async Task<IActionResult> Remover(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);

            if (comprador == null)
                return BadRequest(new { message = "Utilizador n�o � comprador registado." });

            var sucesso = await _favoritoService.RemoverFavoritoAsync(comprador.Id, veiculoId);
            if (!sucesso)
                return BadRequest(new { message = "N�o foi poss�vel remover o favorito." });

            return Ok(new { message = "Favorito removido com sucesso." });
        }

        /// <summary>
        /// GET: api/favoritos/verificar/{veiculoId}
        /// Verifica se um ve�culo est� nos favoritos.
        /// </summary>
        [HttpGet("verificar/{veiculoId}")]
        public async Task<IActionResult> Verificar(int veiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);

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

            var comprador = await _profileService.GetCompradorByUserIdAsync(user.Id);

            if (comprador == null)
                return BadRequest(new { message = "Utilizador n�o � comprador registado." });

            var favoritos = await _favoritoService.ListarFavoritosAsync(comprador.Id, page, 20);
            var total = await _favoritoService.ContarFavoritosAsync(comprador.Id);

            return Ok(new { favoritos, total, page });
        }
    }
}
