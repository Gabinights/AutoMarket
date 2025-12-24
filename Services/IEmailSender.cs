namespace AutoMarket.Services
{
    /// <summary>
    /// Interface for sending emails.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="message">Email body (HTML or plain text).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendEmailAsync(string to, string subject, string message, CancellationToken cancellationToken = default);
    }
}

