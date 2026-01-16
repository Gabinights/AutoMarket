using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Constants;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de notificações.
    /// Cria notificações in-app e pode enviar emails.
    /// </summary>
    public class NotificacaoService : INotificacaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            IEmailSender emailSender,
            ILogger<NotificacaoService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task NotificarUtilizadorAsync(
            string utilizadorId,
            string tipo,
            string assunto,
            string corpo,
            string? linkRelacionado = null,
            string? entidadeRelacionadaId = null,
            string? tipoEntidadeRelacionada = null)
        {
            try
            {
                var utilizador = await _userManager.FindByIdAsync(utilizadorId);
                if (utilizador == null)
                {
                    _logger.LogWarning("Tentativa de notificar utilizador inexistente: {UtilizadorId}", utilizadorId);
                    return;
                }

                // Criar notificação in-app
                var notificacao = new Notificacao
                {
                    DestinatarioId = utilizadorId,
                    Tipo = tipo,
                    Assunto = assunto,
                    Corpo = corpo,
                    LinkRelacionado = linkRelacionado,
                    EntidadeRelacionadaId = entidadeRelacionadaId,
                    TipoEntidadeRelacionada = tipoEntidadeRelacionada,
                    DataCriacao = DateTime.UtcNow,
                    Lida = false
                };

                _context.Notificacoes.Add(notificacao);
                await _context.SaveChangesAsync();

                // Enviar email tambem (opcional)
                if (utilizador.EmailConfirmed)
                {
                    await _emailSender.SendEmailAsync(
                        utilizador.Email ?? "",
                        assunto,
                        $"<p>{corpo}</p>{(linkRelacionado != null ? $"<p><a href='{linkRelacionado}'>Ver detalhes</a></p>" : "")}");
                }

                _logger.LogInformation(
                    "Notifica��o criada para {UtilizadorId}: {Tipo}",
                    utilizadorId, tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar utilizador {UtilizadorId}", utilizadorId);
            }
        }

        public async Task NotificarAdminsAsync(
            string tipo,
            string assunto,
            string corpo,
            string? linkRelacionado = null,
            string? entidadeRelacionadaId = null,
            string? tipoEntidadeRelacionada = null)
        {
            try
            {
                // Obter todos os admins
                var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

                foreach (var admin in admins)
                {
                    await NotificarUtilizadorAsync(
                        admin.Id,
                        tipo,
                        assunto,
                        corpo,
                        linkRelacionado,
                        entidadeRelacionadaId,
                        tipoEntidadeRelacionada);
                }

                _logger.LogInformation(
                    "Notificações enviadas para {AdminCount} admins: {Tipo}",
                    admins.Count, tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar admins para {Tipo}", tipo);
            }
        }

        public async Task<bool> MarcarComolida(int notificacaoId)
        {
            var notificacao = await _context.Notificacoes.FirstOrDefaultAsync(n => n.Id == notificacaoId);
            if (notificacao == null) return false;

            notificacao.Lida = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Notificacao>> ObterNaolidasAsync(string utilizadorId)
        {
            return await _context.Notificacoes
                .Where(n => n.DestinatarioId == utilizadorId && !n.Lida)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Notificacao>> ListarNotificacoesAsync(string utilizadorId, int page = 1, int pageSize = 20)
        {
            return await _context.Notificacoes
                .Where(n => n.DestinatarioId == utilizadorId)
                .OrderByDescending(n => n.DataCriacao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> ContarNotificacoesAsync(string utilizadorId)
        {
            return await _context.Notificacoes
                .CountAsync(n => n.DestinatarioId == utilizadorId);
        }
    }
}
