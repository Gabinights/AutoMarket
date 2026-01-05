using AutoMarket.Models.Entities;

namespace AutoMarket.Services.Interfaces
{
    public interface IEmailAuthService
    {
        /// <summary>
        /// Gera o template e envia o email de confirmação de conta.
        /// </summary>
        Task EnviarEmailConfirmacaoAsync(Utilizador user, string confirmationLink);
    }
}
