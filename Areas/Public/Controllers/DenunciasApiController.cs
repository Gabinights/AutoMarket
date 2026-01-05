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
    public class DenunciasApiController : ControllerBase
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IDenunciaService _denunciaService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DenunciasApiController> _logger;

        public DenunciasApiController(
            UserManager<Utilizador> userManager,
            IDenunciaService denunciaService,
            ApplicationDbContext context,
            ILogger<DenunciasApiController> logger)
        {
            _userManager = userManager;
            _denunciaService = denunciaService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/denuncias/criar
        /// Criar uma denúncia.
        /// </summary>
        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromBody] CreateDenunciaDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Motivo))
                return BadRequest(new { message = "Motivo é obrigatório." });

            if (!dto.VeiculoId.HasValue && string.IsNullOrEmpty(dto.TargetUserId))
                return BadRequest(new { message = "Forneça um veículo ou utilizador para denunciar." });

            string? targetUserId = null;

            // Se denunciar veículo, validar que existe
            if (dto.VeiculoId.HasValue)
            {
                var veiculo = await _context.Veiculos.FindAsync(dto.VeiculoId.Value);
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
