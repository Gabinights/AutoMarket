using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.ViewModels
{
    /// <summary>
    /// ViewModel para alteração de password do utilizador
    /// </summary>
    public class AlterarPasswordViewModel
    {
        [Required(ErrorMessage = "A password atual é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password Atual")]
        public string PasswordAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova password é obrigatória.")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 16)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Password")]
        public string NovaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da password é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Password")]
        [Compare("NovaPassword", ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmarNovaPassword { get; set; } = string.Empty;
    }
}
