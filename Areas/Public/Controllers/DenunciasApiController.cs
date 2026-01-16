using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    [Route("api/[controller]")]
    [ApiController]
    public class DenunciasApiController : ControllerBase
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IVeiculoService _veiculoService;
        private readonly IDenunciaService _denunciaService;
        private readonly ILogger<DenunciasApiController> _logger;

        public DenunciasApiController(
            UserManager<Utilizador> userManager,
            IVeiculoService veiculoService,
            IDenunciaService denunciaService,
            ILogger<DenunciasApiController> logger)
        {
            _userManager = userManager;
            _veiculoService = veiculoService;
            _denunciaService = denunciaService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/denuncias/criar
        /// Criar uma denuncia.
        /// </summary>
        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromBody] CreateDenunciaDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Motivo))
                return BadRequest(new { message = "Motivo obrigatório." });

            if (!dto.VeiculoId.HasValue && string.IsNullOrEmpty(dto.TargetUserId))
                return BadRequest(new { message = "Forneça um veículo ou utilizador para denunciar." });

            string? targetUserId = null;

            // Se denunciar veículo, validar que existe
            if (dto.VeiculoId.HasValue)
            {
                var veiculo = await _veiculoService.GetVeiculoEntityAsync(dto.VeiculoId.Value);
                if (veiculo == null)
                    return BadRequest(new { message = "Veículo não encontrado." });
            }
            // Se denunciar utilizador, converter email para ID
            else if (!string.IsNullOrEmpty(dto.TargetUserId))
            {
                var targetUser = await _userManager.FindByEmailAsync(dto.TargetUserId);
                if (targetUser == null)
                    return BadRequest(new { message = "Utilizador não encontrado." });
                targetUserId = targetUser.Id;
            }

            var denuncia = await _denunciaService.CriarDenunciaAsync(
                user.Id,
                dto.VeiculoId,
                targetUserId,
                dto.Motivo);

            return Ok(new { message = "Denúncia criada com sucesso!", denunciaId = denuncia.Id });
        }
    }

    public class CreateDenunciaDto
    {
        public int? VeiculoId { get; set; }
        public string? TargetUserId { get; set; }
        public string Motivo { get; set; } = "";
    }
}
