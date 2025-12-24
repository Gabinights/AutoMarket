namespace AutoMarket.Services
{
    /// <summary>
    /// Interface for queuing failed emails for retry.
    /// </summary>
    public interface IEmailQueue
    {
        /// <summary>
        /// Queues an email for retry.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="message">Email body.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task QueueEmailAsync(string to, string subject, string message);
    }
}

