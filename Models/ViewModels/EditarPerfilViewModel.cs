using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.ViewModels
{
    /// <summary>
    /// ViewModel para edição de dados pessoais do perfil do utilizador
    /// </summary>
    public class EditarPerfilViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A morada é obrigatória.")]
        [StringLength(200, ErrorMessage = "A morada não pode exceder 200 caracteres.")]
        [Display(Name = "Morada")]
        public string Morada { get; set; } = string.Empty;

        [Required(ErrorMessage = "O contacto é obrigatório.")]
        [Phone(ErrorMessage = "Contacto inválido.")]
        [StringLength(50, ErrorMessage = "O contacto não pode exceder 50 caracteres.")]
        [Display(Name = "Contacto")]
        public string Contacto { get; set; } = string.Empty;

        [StringLength(9, MinimumLength = 9, ErrorMessage = "O NIF deve ter 9 dígitos.")]
        [Display(Name = "NIF (Número de Identificação Fiscal)")]
        public string? NIF { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty; // Read-only (não pode alterar email)

        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; }
    }
}
