using System.ComponentModel.DataAnnotations;
using AutoMarket.Models.Enums;
using AutoMarket.Attributes;

namespace AutoMarket.Models.ViewModels
{
    public class RegisterViewModel : IValidatableObject
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
        [StringLength(200, ErrorMessage= "A morada não pode ter mais de 200 caracteres.")]
        [Display(Name = "Morada")]
        public string Morada { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "O contacto não pode ter mais de 50 caracteres.")]
        [Display(Name = "Contacto")]
        public string Contacto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Por facor, selecione o tipo de conta.")]
        [Display(Name = "Tipo de Conta")]
        public TipoConta TipoConta { get; set; } = TipoConta.Comprador;

        // --- Campos específicos para Vendedor --- (Nullable para não dar erro de validação ao comprador)
        [Display(Name ="NIF")]
        [NifPortugues]
        public string? NIF { get; set; }

        // -- Lógica de validação personalizada do NIF --
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Regra 1: Validação para empresas
            if (TipoConta == TipoConta.Empresa)
            {
                if (string.IsNullOrWhiteSpace(NIF))
                {
                    yield return new ValidationResult("O NIF é obrigatório para contas de Empresa.", new[] { nameof(NIF) });
                }
                else if (!NIF.Trim().StartsWith("5"))
                {
                    yield return new ValidationResult("NIF de empresa deve começar por 5.", new[] { nameof(NIF) });
                }
            }
        }
    }
}
