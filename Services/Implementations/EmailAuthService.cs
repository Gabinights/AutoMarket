using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;

namespace AutoMarket.Services.Implementations
{
    public class EmailAuthService : IEmailAuthService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailAuthService> _logger;

        public EmailAuthService(
            IEmailSender emailSender,
            ILogger<EmailAuthService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task EnviarEmailConfirmacaoAsync(Utilizador user, string confirmationLink)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("Utilizador {UserId} sem email definido.", user.Id);
                return;
            }

            try
            {
                var emailBody = EmailTemplateService.GenerateEmailConfirmationTemplate(user.Nome, confirmationLink);

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Bem-vindo ao AutoMarket - Confirme a sua conta",
                    emailBody
                );

                _logger.LogInformation("Email de confirmação enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email para {Email}", user.Email);
            }
        }
    }
}
