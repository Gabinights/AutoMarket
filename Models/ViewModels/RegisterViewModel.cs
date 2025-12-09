using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Morada { get; set; } = string.Empty;

        [Required]
        public string Contactos { get; set; } = string.Empty;

        [Display(Name = "Quero vender carros?")]
        public bool IsVendedor { get; set; } //Atualizar View

        // --- Campos específicos para Vendedor --- (Nullable para não dar erro de validação ao comprador)
        public string? NIF { get; set; }
        public bool? IsEmpresa { get; set; }
    }
}
