using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.ViewModels
{
    public class RegisterViewModel
    {
        // --- Identity Base ---
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar password")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;




        [Required]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Morada { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contacto")]
        public string Contacto { get; set; } = string.Empty;

        [Display(Name = "Quero vender carros?")]
        public bool IsVendedor { get; set; } //Atualizar View

        // --- Campos específicos para Vendedor --- (Nullable para não dar erro de validação ao comprador)
        public string? NIF { get; set; }
        public bool IsEmpresa { get; set; }
    }
}
