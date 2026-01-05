using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de denúncias.
    /// Gerencia workflow completo de denúncias com auditoria.
    /// </summary>
    public class DenunciaService : IDenunciaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditoriaService _auditoriaService;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<DenunciaService> _logger;

        public DenunciaService(
            ApplicationDbContext context,
            IAuditoriaService auditoriaService,
            INotificacaoService notificacaoService,
            ILogger<DenunciaService> logger)
        {
            _context = context;
            _auditoriaService = auditoriaService;
            _notificacaoService = notificacaoService;
            _logger = logger;
        }

        public async Task<Denuncia> CriarDenunciaAsync(
            string denuncianteId,
            int? veiculoId = null,
            string? targetUserId = null,
            string motivo = "")
        {
            // Validação: deve ter alvo (veículo ou utilizador)
            if (!veiculoId.HasValue && string.IsNullOrEmpty(targetUserId))
            {
                throw new ArgumentException("Denúncia deve ter um alvo: veículo ou utilizador.");
            }

            var denuncia = new Denuncia
            {
                DenuncianteId = denuncianteId,
                TargetVeiculoId = veiculoId,
                TargetUserId = targetUserId,
                Motivo = motivo,
                Estado = EstadoDenuncia.Aberta,
                DataCriacao = DateTime.UtcNow
            };

            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Denúncia #{DenunciaId} criada pelo utilizador {DenuncianteId}",
                denuncia.Id, denuncianteId);

            // Notificar admin (todos com role Admin)
            await _notificacaoService.NotificarAdminsAsync(
                tipo: "DENUNCIA_ABERTA",
                assunto: $"Nova Denúncia #{denuncia.Id}",
                corpo: $"Denúncia contra {(veiculoId.HasValue ? "veículo" : "utilizador")} recebida.",
                linkRelacionado: $"/Admin/Denuncias/{denuncia.Id}",
                entidadeRelacionadaId: denuncia.Id.ToString(),
                tipoEntidadeRelacionada: "Denuncia");

            return denuncia;
        }

        public async Task<Denuncia?> ObterDenunciaAsync(int denunciaId)
        {
            return await _context.Denuncias
                .Include(d => d.Denunciante)
                .Include(d => d.TargetVeiculo)
                    .ThenInclude(v => v!.Vendedor)
                        .ThenInclude(vnd => vnd.User)
                .Include(d => d.TargetUser)
                .Include(d => d.AnalisadoPorAdmin)
                .FirstOrDefaultAsync(d => d.Id == denunciaId);
        }

        public async Task<List<Denuncia>> ListarDenunciasAsync(
            string? estado = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Denuncias
                .Include(d => d.Denunciante)
                .Include(d => d.TargetVeiculo)
                .Include(d => d.TargetUser)
                .Include(d => d.AnalisadoPorAdmin)
                .OrderByDescending(d => d.DataCriacao)
                .AsQueryable();

            // Filtrar por estado se fornecido
            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDenuncia>(estado, true, out var estadoParsed))
            {
                query = query.Where(d => d.Estado == estadoParsed);
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IniciarAnaliseAsync(int denunciaId, string adminId)
        {
            var denuncia = await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == denunciaId);
            if (denuncia == null)
            {
                _logger.LogWarning("Tentativa de analisar denúncia inexistente #{DenunciaId}", denunciaId);
                return false;
            }

            if (denuncia.Estado != EstadoDenuncia.Aberta)
            {
                _logger.LogWarning(
                    "Tentativa de analisar denúncia #{DenunciaId} em estado {Estado}",
                    denunciaId, denuncia.Estado);
                return false;
            }

            var estadoAnterior = denuncia.Estado.ToString();
            denuncia.Estado = EstadoDenuncia.EmAnalise;
            denuncia.AnalisadoPorAdminId = adminId;

            await _context.SaveChangesAsync();

            // Registar auditoria
            await _auditoriaService.RegistarAcaoAsync(
                adminId,
                "DENUNCIA_ANALISE_INICIADA",
                $"Análise de denúncia #{denunciaId} iniciada",
                entidadeAfetadaId: denunciaId.ToString(),
                tipoEntidade: "Denuncia",
                dadosAntigos: JsonSerializer.Serialize(new { Estado = estadoAnterior }),
                dadosNovos: JsonSerializer.Serialize(new { Estado = EstadoDenuncia.EmAnalise.ToString() }));

            _logger.LogInformation(
                "Admin {AdminId} iniciou análise de denúncia #{DenunciaId}",
                adminId, denunciaId);

            return true;
        }

        public async Task<bool> EncerrarDenunciaAsync(
            int denunciaId,
            string adminId,
            bool procedente,
            string decisao)
        {
            var denuncia = await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == denunciaId);
            if (denuncia == null)
            {
                _logger.LogWarning("Tentativa de encerrar denúncia inexistente #{DenunciaId}", denunciaId);
                return false;
            }

            if (denuncia.Estado == EstadoDenuncia.Encerrada)
            {
                _logger.LogWarning("Tentativa de encerrar denúncia já encerrada #{DenunciaId}", denunciaId);
                return false;
            }

            var estadoAnterior = denuncia.Estado.ToString();
            denuncia.Estado = EstadoDenuncia.Encerrada;
            denuncia.AnalisadoPorAdminId = adminId;
            denuncia.DecisaoAdmin = $"{(procedente ? "PROCEDENTE" : "NÃO_PROCEDENTE")}: {decisao}";

            await _context.SaveChangesAsync();

            // Registar auditoria
            await _auditoriaService.RegistarAcaoAsync(
                adminId,
                "DENUNCIA_ENCERRADA",
                $"Denúncia #{denunciaId} encerrada como {(procedente ? "PROCEDENTE" : "NÃO_PROCEDENTE")}",
                entidadeAfetadaId: denunciaId.ToString(),
                tipoEntidade: "Denuncia",
                dadosAntigos: JsonSerializer.Serialize(new { Estado = estadoAnterior }),
                dadosNovos: JsonSerializer.Serialize(new { Estado = EstadoDenuncia.Encerrada.ToString(), DecisaoAdmin = denuncia.DecisaoAdmin }));

            // Notificar denunciante
            await _notificacaoService.NotificarUtilizadorAsync(
                denuncia.DenuncianteId,
                tipo: "DENUNCIA_ENCERRADA",
                assunto: $"Denúncia #{denunciaId} Encerrada",
                corpo: $"Sua denúncia foi analisada e considerada {(procedente ? "procedente" : "não procedente")}.",
                linkRelacionado: $"/Admin/Denuncias/{denunciaId}");

            // Notificar alvo da denúncia (se for utilizador)
            if (!string.IsNullOrEmpty(denuncia.TargetUserId))
            {
                await _notificacaoService.NotificarUtilizadorAsync(
                    denuncia.TargetUserId,
                    tipo: "DENUNCIA_RESULTADO",
                    assunto: $"Resultado de Denúncia contra você",
                    corpo: procedente
                        ? "Uma denúncia contra você foi considerada procedente. Contacte o suporte."
                        : "Uma denúncia contra você foi investigada e considerada não procedente.",
                    linkRelacionado: $"/Admin/Denuncias/{denunciaId}");
            }

            _logger.LogInformation(
                "Admin {AdminId} encerrou denúncia #{DenunciaId} como {Resultado}",
                adminId, denunciaId, procedente ? "PROCEDENTE" : "NÃO_PROCEDENTE");

            return true;
        }

        public async Task<Dictionary<string, int>> ContarPorEstadoAsync()
        {
            var contagens = await _context.Denuncias
                .GroupBy(d => d.Estado)
                .Select(g => new { Estado = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Estado, x => x.Count);

            return contagens;
        }
    }
}
