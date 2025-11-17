using AutoMarket.Models.ViewModels;

namespace AutoMarket.Services
{
    /// <summary>
    /// Service for generating email templates using Razor views.
    /// </summary>
    public class EmailTemplateService
    {
        private readonly ViewRenderService _viewRenderService;

        public EmailTemplateService(ViewRenderService viewRenderService)
        {
            _viewRenderService = viewRenderService;
        }

        /// <summary>
        /// Generates an HTML email template for email confirmation using Razor view.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="confirmationLink">The email confirmation link.</param>
        /// <param name="httpContext">The HTTP context for view rendering.</param>
        /// <returns>HTML email template as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when userName or confirmationLink is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when confirmationLink is not a valid URL.</exception>
        public async Task<string> GenerateEmailConfirmationTemplateAsync(string userName, string confirmationLink, HttpContext httpContext)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName), "User name cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(confirmationLink))
                throw new ArgumentNullException(nameof(confirmationLink), "Confirmation link cannot be null or empty.");

            if (!Uri.TryCreate(confirmationLink, UriKind.Absolute, out _))
                throw new ArgumentException("Confirmation link must be a valid absolute URL.", nameof(confirmationLink));

            var viewModel = new EmailConfirmationViewModel
            {
                UserName = userName,
                ConfirmationLink = confirmationLink
            };

            try
            {
                return await _viewRenderService.RenderToStringAsync("~/Views/Emails/EmailConfirmation.cshtml", viewModel, httpContext);
            }
            catch
            {
                // Fallback: try without path prefix
                return await _viewRenderService.RenderToStringAsync("Emails/EmailConfirmation", viewModel, httpContext);
            }
        }

        /// <summary>
        /// Generates an HTML email template for email confirmation (fallback method using string interpolation).
        /// This method is kept for backward compatibility and as a fallback if Razor view rendering fails.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="confirmationLink">The email confirmation link.</param>
        /// <returns>HTML email template as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when userName or confirmationLink is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when confirmationLink is not a valid URL.</exception>
        [Obsolete("Use GenerateEmailConfirmationTemplateAsync instead. This method is kept for backward compatibility.")]
        public static string GenerateEmailConfirmationTemplate(string userName, string confirmationLink)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName), "User name cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(confirmationLink))
                throw new ArgumentNullException(nameof(confirmationLink), "Confirmation link cannot be null or empty.");

            if (!Uri.TryCreate(confirmationLink, UriKind.Absolute, out _))
                throw new ArgumentException("Confirmation link must be a valid absolute URL.", nameof(confirmationLink));

            // HTML encode user input to prevent XSS
            var encodedUserName = System.Net.WebUtility.HtmlEncode(userName);
            var encodedConfirmationLink = System.Net.WebUtility.HtmlEncode(confirmationLink);

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        .link-text {{ word-break: break-all; color: #007bff; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Bem-vindo ao AutoMarket!</h1>
                        </div>
                        <div class=""content"">
                            <p>Olá {encodedUserName},</p>
                            <p>Obrigado por se registar no AutoMarket. Para completar o seu registo, por favor confirme o seu endereço de email clicando no botão abaixo:</p>
                            <p style=""text-align: center;"">
                                <a href=""{confirmationLink}"" class=""button"">Confirmar Email</a>
                            </p>
                            <p>Ou copie e cole o seguinte link no seu navegador:</p>
                            <p class=""link-text"">{encodedConfirmationLink}</p>
                            <p>Este link expirará em 24 horas.</p>
                            <p>Se não criou uma conta no AutoMarket, pode ignorar este email.</p>
                        </div>
                        <div class=""footer"">
                            <p>&copy; 2025 AutoMarket. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}

