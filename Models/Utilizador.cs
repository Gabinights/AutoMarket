using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Models
{// Utilizador é a classe base para todos os tipos de utilizadores no sistema
    public class Utilizador : IdentityUser // Identity User já inclui os campos básicos como Id, UserName, Email, PasswordHash, etc.
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome Completo")]
        [PersonalData]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "A morada não pode exceder 200 caracteres.")]
        [PersonalData]
        public string? Morada { get; set; }
            
        [DataType(DataType.Date)]
        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.UtcNow;

        [MaxLength(50, ErrorMessage = "Os contactos não podem exceder 50 caracteres.")]
        [PersonalData]
        public string Contacto { get; set; } = string.Empty;

        [StringLength(9)]
        [PersonalData]
        public string? NIF { get; set; }
        // POde ser null se o utilizador ainda não tiver preenchido os dados fiscais

        //Campos para o bloqueio administrativo 
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason { get; set; }    

        // soft Delete
        public bool IsDeleted { get; set; } = false;
    }
}

