using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de auditoria.
    /// </summary>
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditoriaService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task RegistarAcaoAsync(
            string adminId,
            string tipoAcao,
            string descricao,
            string? entidadeAfetadaId = null,
            string? tipoEntidade = null,
            string? dadosAntigos = null,
            string? dadosNovos = null,
            string? enderecoIP = null,
            string? userAgent = null)
        {
            try
            {
                // Obter contexto HTTP se não fornecido
                var httpContext = _httpContextAccessor.HttpContext;
                enderecoIP ??= httpContext?.Connection.RemoteIpAddress?.ToString();
                userAgent ??= httpContext?.Request.Headers["User-Agent"].ToString();

                var auditoria = new AuditoriaLog
                {
                    AdminId = adminId,
                    TipoAcao = tipoAcao,
                    Descricao = descricao,
                    EntidadeAfetadaId = entidadeAfetadaId,
                    TipoEntidade = tipoEntidade,
                    DadosAntigos = dadosAntigos,
                    DadosNovos = dadosNovos,
                    EnderecoIP = enderecoIP,
                    UserAgent = userAgent,
                    DataHora = DateTime.UtcNow
                };

                _context.AuditoriaLogs.Add(auditoria);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Auditoria registada: {TipoAcao} por {AdminId} em {DataHora}",
                    tipoAcao, adminId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registar auditoria para {TipoAcao}", tipoAcao);
                // Não lançar exceção - auditoria não deve bloquear operação principal
            }
        }

        public async Task<List<AuditoriaLog>> ListarLogsAsync(int page = 1, int pageSize = 50)
        {
            return await _context.AuditoriaLogs
                .Include(a => a.Admin)
                .OrderByDescending(a => a.DataHora)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<AuditoriaLog>> ListarLogsPorAdminAsync(string adminId, int page = 1, int pageSize = 50)
        {
            return await _context.AuditoriaLogs
                .Where(a => a.AdminId == adminId)
                .OrderByDescending(a => a.DataHora)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
