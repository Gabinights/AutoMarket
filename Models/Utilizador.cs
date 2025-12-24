using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Models
{// Utilizador é a classe base para todos os tipos de utilizadores no sistema
    public class Utilizador : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A morada é obrigatória.")]
        [MaxLength(200, ErrorMessage = "A morada não pode exceder 200 caracteres.")]
        public string Morada { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.UtcNow;

        [MaxLength(50, ErrorMessage = "Os contactos não podem exceder 50 caracteres.")]
        public string Contacto { get; set; } = string.Empty;

        //Campo para o bloqueio administrativo 
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason;    
    }
}