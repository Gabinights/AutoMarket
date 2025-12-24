namespace AutoMarket.Models.ViewModels
{
    /// <summary>
    /// View model for email confirmation template.
    /// </summary>
    public class EmailConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email confirmation link.
        /// </summary>
        public string ConfirmationLink { get; set; } = string.Empty;
    }
}

