using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Quero vender carros?")]
        public bool IsVendedor { get; set; } //TODO: Atualizar View

        [Display(Name = "Sou uma Empresa/Stand?")]
        public bool IsEmpresa { get; set; } //TODO: Atualizar View

        // --- Campos específicos para Vendedor --- (Nullable para não dar erro de validação ao comprador)
        [Display(Name ="NIF")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "O NIF deve ter 9 dígitos.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage ="O NIF deve conter apenas números.")]
        public string? NIF { get; set; }

        // -- Lógica de validação personalizada --
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsVendedor && IsEmpresa)
            {
                if (string.IsNullOrWhiteSpace(NIF))
                {
                    yield return new ValidationResult(
                        "O NIF é obrigatório para contas de Empresa.",
                        new[] { nameof(NIF) });
                }
                else if (!NIF.StartsWith("5"))
                {
                    // Validar se começa por 5 (empresas portuguesas)
                    yield return new ValidationResult(
                        "O NIF de empresa deve começar por 5.",
                        new[] { nameof(NIF) });
                }
            }
            // Se for vendedor particular, NIF é opcional, mas se preencher deve ser válido 
        }

    }
}
