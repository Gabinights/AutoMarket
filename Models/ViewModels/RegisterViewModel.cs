using System.ComponentModel.DataAnnotations;
using AutoMarket.Models.Enums;
using AutoMarket.Utils;

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

        [Required(ErrorMessage = "A confirmação da password é obrigatória.")]
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

        [Required(ErrorMessage = "Por favor, selecione o tipo de conta.")]
        [Display(Name = "Tipo de Conta")]
        public TipoConta TipoConta { get; set; } = TipoConta.Comprador;

        [Display(Name = "NIF")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "O NIF deve ter 9 dígitos")]
        public string? NIF { get; set; }

        // -- Lógica de validação personalizada do NIF --
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TipoConta == TipoConta.Empresa)
            {
                if (string.IsNullOrWhiteSpace(NIF))
                {
                    yield return new ValidationResult(
                        "O NIF é obrigatório para contas empresariais.", new[] { nameof(NIF) });
                }
                else if (!NifValidator.IsValid(NIF))
                {
                    yield return new ValidationResult(
                        "NIF inválido.", new[] { nameof(NIF) });
                }
                else if (!NifValidator.IsEmpresa(NIF))
                {
                    yield return new ValidationResult(
                    "Selecionou 'Empresa', mas introduziu um NIF de Pessoa Singular. (NIFs de empresa começam por 5, 6 ou 9)",
                    new[] { nameof(NIF) });
                }
            }
            else if (TipoConta == TipoConta.Vendedor)
            {
                if (!string.IsNullOrWhiteSpace(NIF))
                {
                    if (!NifValidator.IsValid(NIF))
                    {
                        yield return new ValidationResult(
                            "NIF inválido.", new[] { nameof(NIF) });
                    }
                    else if (!NifValidator.IsParticular(NIF))
                    {
                        // Nota: Podes ser mais permissivo aqui se quiseres permitir que uma empresa se registe como "user normal", 
                        // mas se a distinção for rígida, usa este bloco.
                        yield return new ValidationResult(
                            "Para contas particulares, o NIF deve começar por 1, 2 ou 3.",
                            new[] { nameof(NIF) });
                    }
                }
            }
            // Se houver mais tipos, adicionar mais 'else if'
        }
    }
}
