using AutoMarket.Models;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Services
{
    public class EmailAuthService : IEmailAuthService
    {
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly ILogger<EmailAuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        public EmailAuthService(
            IEmailSender emailSender,
            EmailTemplateService emailTemplateService,
            ILogger<EmailAuthService> logger,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment)
        {
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task EnviarEmailConfirmacaoAsync(Utilizador user, string confirmationLink)
        {   // Guard Clause
            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("Tentativa de enviar email para utilizador {UserId} sem email definido.", user.Id);
                return; // Ou lançar exceção: throw new InvalidOperationException("User email cannot be null");
            }

            try
            {
                // Se estivermos fora de um pedido HTTP (ex: background job), isto pode ser null, 
                // mas no fluxo de Registo existe sempre.
                // passar o contexto
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    {
                        _logger.LogWarning("HttpContext não disponível para renderizar template de email para {Email}", user.Email);
                        // Consider using the static fallback method or a pre-rendered template
                        return;
                    }

                var emailBody = await _emailTemplateService.GenerateEmailConfirmationTemplateAsync(
                    user.Nome,
                    confirmationLink,
                    httpContext 
                );

                // 2. Enviar
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Bem-vindo ao AutoMarket - Confirme a sua conta",
                    emailBody
                );

                _logger.LogInformation("Email de confirmação enviado para {Email}", user.Email);
                
                // Em desenvolvimento, logar o link de confirmação para facilitar testes
                if (_environment.IsDevelopment())
                {
                    _logger.LogWarning("===========================================");
                    _logger.LogWarning("LINK DE CONFIRMAÇÃO (Desenvolvimento):");
                    _logger.LogWarning("{ConfirmationLink}", confirmationLink);
                    _logger.LogWarning("===========================================");
                }
            }
            catch (Exception ex)
            {
                // Logamos o erro aqui para não "rebentar" o registo do utilizador
                _logger.LogError(ex, "Falha crítica ao enviar email para {Email}", user.Email);
                // Não fazemos throw para não crashar o registo do user, apenas logamos o erro de envio.
            }
        }
        
    }
}
