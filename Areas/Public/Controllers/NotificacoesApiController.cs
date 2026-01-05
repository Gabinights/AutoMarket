using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Areas.Public.Controllers
{
    [Authorize]
    [Area("Public")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacoesApiController : ControllerBase
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<NotificacoesApiController> _logger;

        public NotificacoesApiController(
            UserManager<Utilizador> userManager,
            INotificacaoService notificacaoService,
            ILogger<NotificacoesApiController> logger)
        {
            _userManager = userManager;
            _notificacaoService = notificacaoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/notificacoes/nao-lidas
        /// Obter notificações não lidas do utilizador.
        /// </summary>
        [HttpGet("nao-lidas")]
        public async Task<IActionResult> ObterNaoLidas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notificacoes = await _notificacaoService.ObterNaolidasAsync(user.Id);
            return Ok(new { count = notificacoes.Count, notificacoes });
        }

        /// <summary>
        /// GET: api/notificacoes/listar
        /// Listar todas as notificações com paginação.
        /// </summary>
        [HttpGet("listar")]
        public async Task<IActionResult> Listar(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notificacoes = await _notificacaoService.ListarNotificacoesAsync(user.Id, page, 20);
            return Ok(new { notificacoes, page });
        }

        /// <summary>
        /// POST: api/notificacoes/marcar-como-lida/{id}
        /// Marcar uma notificação como lida.
        /// </summary>
        [HttpPost("marcar-como-lida/{id}")]
        public async Task<IActionResult> MarcarComoLida(int id)
        {
            var sucesso = await _notificacaoService.MarcarComolida(id);
            if (!sucesso)
                return BadRequest(new { message = "Não foi possível marcar a notificação como lida." });

            return Ok(new { message = "Notificação marcada como lida." });
        }
    }
}
