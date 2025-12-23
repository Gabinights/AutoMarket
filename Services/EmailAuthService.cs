using AutoMarket.Models;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services; // Para o IEmailSender


namespace AutoMarket.Services
{
    public class EmailAuthService : IEmailAuthService
    {
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly ILogger<EmailAuthService> _logger;

        public EmailAuthService(
            IEmailSender emailSender,
            EmailTemplateService emailTemplateService,
            ILogger<EmailAuthService> logger)
        {
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public async Task EnviarEmailConfirmacaoAsync(Utilizador user, string confirmationLink)
        {
            try
            {
                // 1. Gerar o corpo do email (HTML)
                // Nota: O TemplateService pode precisar de ajuste se usava HttpContext, 
                // mas assumindo que ele gera HTML string puro:
                var emailBody = await _emailTemplateService.GenerateEmailConfirmationTemplateAsync(
                    user.Nome,
                    confirmationLink,
                    null // Se o teu template service não depender estritamente do contexto para imagens, passa null
                );

                // 2. Enviar
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Bem-vindo ao AutoMarket - Confirme a sua conta",
                    emailBody
                );

                _logger.LogInformation("Email de confirmação enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                // Logamos o erro aqui para não "rebentar" o registo do utilizador
                _logger.LogError(ex, "Falha crítica ao enviar email para {Email}", user.Email);
                // Opcional: Relançar a exceção se quiseres que o Controller saiba que falhou
                // throw; 
            }
        }
    }
}
