using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Services
{
    /// <summary>
    /// Service for sending emails using SMTP via MailKit.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailFailureTracker _failureTracker;
        private int _failureCount = 0;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger, EmailFailureTracker failureTracker)
        {
            _configuration = configuration;
            _logger = logger;
            _failureTracker = failureTracker;
        }

        public async Task SendEmailAsync(string to, string subject, string message, CancellationToken cancellationToken = default)
        {
            // Circuit breaker: fail fast if too many recent failures
            if (_failureTracker.IsCircuitOpen())
            {
                _logger.LogWarning("Email circuit breaker is OPEN. Skipping email send to {To}. Total failures: {FailureCount}", 
                    to, _failureTracker.GetFailureCount());
                throw new InvalidOperationException("Email service is temporarily unavailable due to repeated failures. Please try again later.");
            }

            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = emailSettings.GetValue<int>("SmtpPort", 587);
                var smtpUsername = emailSettings["SmtpUsername"];
                var smtpPassword = emailSettings["SmtpPassword"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"] ?? "AutoMarket";
                
                // Get SecureSocketOptions from configuration (defaults to StartTls)
                var secureSocketOptionString = emailSettings["SecureSocketOptions"] ?? "StartTls";
                var secureSocketOptions = ParseSecureSocketOptions(secureSocketOptionString);

                // If email settings are not configured, log and return (for development)
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email settings not configured. Email not sent to {To}. Subject: {Subject}", to, subject);
                    _logger.LogInformation("Email would be sent to: {To}\nSubject: {Subject}\nMessage: {Message}", to, subject, message);
                    return;
                }

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message
                };
                email.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                try
                {
                    await client.ConnectAsync(smtpServer, smtpPort, secureSocketOptions, cancellationToken);
                }
                catch (SmtpCommandException ex)
                {
                    _logger.LogError(ex, "SMTP connection error: Failed to connect to {Server}:{Port} with {SecureOption}", smtpServer, smtpPort, secureSocketOptions);
                    RecordFailure();
                    throw new InvalidOperationException($"Failed to connect to SMTP server {smtpServer}:{smtpPort}", ex);
                }
                catch (SmtpProtocolException ex)
                {
                    _logger.LogError(ex, "SMTP protocol error: Failed to connect to {Server}:{Port}", smtpServer, smtpPort);
                    RecordFailure();
                    throw new InvalidOperationException($"SMTP protocol error connecting to {smtpServer}:{smtpPort}", ex);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Email connection was cancelled for {To}", to);
                    throw;
                }

                try
                {
                    await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
                }
                catch (AuthenticationException ex)
                {
                    _logger.LogError(ex, "SMTP authentication failed on {Server}", smtpServer);
                    RecordFailure();
                    throw new UnauthorizedAccessException($"SMTP authentication failed for {smtpServer}", ex);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Email authentication was cancelled for {To}", to);
                    throw;
                }
                
                try
                {
                    await client.SendAsync(email, cancellationToken);
                    _logger.LogInformation("Email sent successfully to {To}", to);
                    _failureTracker.RecordSuccess();
                    _failureCount = 0;
                }
                catch (SmtpCommandException ex)
                {
                    _logger.LogError(ex, "SMTP send error: Failed to send email to {To}", to);
                    RecordFailure();
                    throw new InvalidOperationException($"Failed to send email to {to}", ex);
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Email send operation was cancelled for {To}", to);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Email send failed for {To}. Failure count: {FailureCount}", to, _failureTracker.GetFailureCount());
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Email authentication failed for {To}. Failure count: {FailureCount}", to, _failureTracker.GetFailureCount());
                throw;
            }
            catch (Exception ex)
            {
                RecordFailure();
                _logger.LogError(ex, "Unexpected error sending email to {To}. Failure count: {FailureCount}", to, _failureTracker.GetFailureCount());
                // Don't throw unexpected errors - we don't want email failures to break user registration
                // In production, you might want to queue the email for retry
            }
        }

        private void RecordFailure()
        {
            _failureTracker.RecordFailure();
            Interlocked.Increment(ref _failureCount);
        }

        /// <summary>
        /// Gets the current failure count for monitoring/metrics.
        /// </summary>
        public int GetFailureCount() => _failureCount;

        private static SecureSocketOptions ParseSecureSocketOptions(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "none" => SecureSocketOptions.None,
                "automatic" => SecureSocketOptions.Auto,
                "starttls" => SecureSocketOptions.StartTls,
                "starttlswhenavailable" => SecureSocketOptions.StartTlsWhenAvailable,
                "ssl" => SecureSocketOptions.SslOnConnect,
                _ => SecureSocketOptions.StartTls // Default
            };
        }
    }
}

