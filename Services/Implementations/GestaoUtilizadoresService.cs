using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Implementacao do serviço de gestao de utilizadores.
    /// </summary>
    public class GestaoUtilizadoresService : IGestaoUtilizadoresService
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IAuditoriaService _auditoriaService;
        private readonly INotificacaoService _notificacaoService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GestaoUtilizadoresService> _logger;

        public GestaoUtilizadoresService(
            UserManager<Utilizador> userManager,
            IAuditoriaService auditoriaService,
            INotificacaoService notificacaoService,
            IMemoryCache cache,
            ILogger<GestaoUtilizadoresService> logger)
        {
            _userManager = userManager;
            _auditoriaService = auditoriaService;
            _notificacaoService = notificacaoService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> BloquearUtilizadorAsync(string utilizadorId, string motivo, string adminId)
        {
            var utilizador = await _userManager.FindByIdAsync(utilizadorId);
            if (utilizador == null)
            {
                _logger.LogWarning("Tentativa de bloquear utilizador inexistente: {UtilizadorId}", utilizadorId);
                return false;
            }

            if (utilizador.IsBlocked)
            {
                _logger.LogWarning("Utilizador {UtilizadorId} ja esta bloqueado", utilizadorId);
                return false;
            }

            var estadoAnterior = utilizador.IsBlocked;
            utilizador.IsBlocked = true;
            utilizador.BlockReason = motivo;

            await _userManager.UpdateSecurityStampAsync(utilizador);

            var result = await _userManager.UpdateAsync(utilizador);
            if (!result.Succeeded)
            {
                _logger.LogError("Erro ao bloquear utilizador {UtilizadorId}", utilizadorId);
                return false;
            }

            // -- Cache Invalidation
            var cacheKey = $"User_Status_{utilizadorId}";
            _cache.Set(cacheKey, false, TimeSpan.FromMinutes(30));

            // Registar auditoria
            await _auditoriaService.RegistarAcaoAsync(
                adminId,
                "USUARIO_BLOQUEADO",
                $"Utilizador {utilizador.Nome} ({utilizador.Email}) bloqueado. Motivo: {motivo}",
                entidadeAfetadaId: utilizadorId,
                tipoEntidade: "Utilizador",
                dadosAntigos: JsonSerializer.Serialize(new { IsBlocked = estadoAnterior, BlockReason = (string?)null }),
                dadosNovos: JsonSerializer.Serialize(new { IsBlocked = true, BlockReason = motivo }));

            // Notificar o utilizador
            await _notificacaoService.NotificarUtilizadorAsync(
                utilizadorId,
                tipo: "USUARIO_BLOQUEADO",
                assunto: "A sua conta foi bloqueada",
                corpo: $"A sua conta foi bloqueada pela administracao. Motivo: {motivo}. Contacte o suporte se julgar que isto e um erro.");

            _logger.LogInformation(
                "Admin {AdminId} bloqueou utilizador {UtilizadorId}. Motivo: {Motivo}",
                adminId, utilizadorId, motivo);

            return true;
        }

        public async Task<bool> DesbloquearUtilizadorAsync(string utilizadorId, string adminId)
        {
            var utilizador = await _userManager.FindByIdAsync(utilizadorId);
            if (utilizador == null)
            {
                _logger.LogWarning("Tentativa de desbloquear utilizador inexistente: {UtilizadorId}", utilizadorId);
                return false;
            }

            if (!utilizador.IsBlocked)
            {
                _logger.LogWarning("Utilizador {UtilizadorId} nao esta bloqueado", utilizadorId);
                return false;
            }

            var motivoAnterior = utilizador.BlockReason;
            utilizador.IsBlocked = false;
            utilizador.BlockReason = null;

            var result = await _userManager.UpdateAsync(utilizador);
            if (!result.Succeeded)
            {
                _logger.LogError("Erro ao desbloquear utilizador {UtilizadorId}", utilizadorId);
                return false;
            }

            // Registar auditoria
            await _auditoriaService.RegistarAcaoAsync(
                adminId,
                "USUARIO_DESBLOQUEADO",
                $"Utilizador {utilizador.Nome} ({utilizador.Email}) desbloqueado",
                entidadeAfetadaId: utilizadorId,
                tipoEntidade: "Utilizador",
                dadosAntigos: JsonSerializer.Serialize(new { IsBlocked = true, BlockReason = motivoAnterior }),
                dadosNovos: JsonSerializer.Serialize(new { IsBlocked = false, BlockReason = (string?)null }));

            // Notificar o utilizador
            await _notificacaoService.NotificarUtilizadorAsync(
                utilizadorId,
                tipo: "USUARIO_DESBLOQUEADO",
                assunto: "A sua conta foi desbloqueada",
                corpo: "A sua conta foi desbloqueada pela administracao. Pode agora aceder normalmente.");

            _logger.LogInformation(
                "Admin {AdminId} desbloqueou utilizador {UtilizadorId}",
                adminId, utilizadorId);

            return true;
        }

        public async Task<string?> ObterMotivoBloqueioAsync(string utilizadorId)
        {
            var utilizador = await _userManager.FindByIdAsync(utilizadorId);
            if (utilizador == null || !utilizador.IsBlocked)
                return null;

            return utilizador.BlockReason;
        }

        public async Task<List<Utilizador>> ListarUtilizadoresBloqueadosAsync(int page = 1, int pageSize = 20)
        {
            var users = await _userManager.Users
                .Where(u => u.IsBlocked && !u.IsDeleted)
                .OrderByDescending(u => u.DataRegisto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return users;
        }
    }
}
