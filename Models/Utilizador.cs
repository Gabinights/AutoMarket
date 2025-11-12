using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Models
{
    public class Utilizador : IdentityUser
    {
        public string Nome { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;
        public string Contactos { get; set; } = string.Empty;
        public string StatusAprovacao { get; set; } = "Pendente";
    }
}
