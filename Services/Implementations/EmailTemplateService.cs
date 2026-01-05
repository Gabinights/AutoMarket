namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Service for generating email templates.
    /// </summary>
    public class EmailTemplateService
    {
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(ILogger<EmailTemplateService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generates an HTML email template for email confirmation.
        /// </summary>
        public static string GenerateEmailConfirmationTemplate(string userName, string confirmationLink)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));

            if (string.IsNullOrWhiteSpace(confirmationLink))
                throw new ArgumentNullException(nameof(confirmationLink));

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
                        <div class=""header""><h1>Bem-vindo ao AutoMarket!</h1></div>
                        <div class=""content"">
                            <p>Olá {encodedUserName},</p>
                            <p>Obrigado por se registar. Confirme o seu email clicando abaixo:</p>
                            <p style=""text-align: center;""><a href=""{encodedConfirmationLink}"" class=""button"">Confirmar Email</a></p>
                            <p>Link: {encodedConfirmationLink}</p>
                            <p>Este link expirará em 24 horas.</p>
                        </div>
                        <div class=""footer""><p>&copy; 2025 AutoMarket.</p></div>
                    </div>
                </body>
                </html>";
        }
    }
}