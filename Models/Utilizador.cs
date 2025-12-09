using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models
{// Utilizador é a classe base para todos os tipos de utilizadores no sistema
    public class Utilizador : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A morada é obrigatória.")]
        [MaxLength(200, ErrorMessage = "A morada não pode exceder 200 caracteres.")]
        public string Morada { get; set; } = string.Empty;

        [RegularExpression(@"^\d{0-9}$", ErrorMessage = "NIF deve ter 9 dígitos")]
        public string NIF { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DataRegisto { get; set; } = DateTime.Now;

        [MaxLength(50, ErrorMessage = "Os contactos não podem exceder 50 caracteres.")]
        public string Contacto { get; set; } = string.Empty;

        //Campo para o bloqueio administrativo 
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason

        //estes métodos vao passar para compradores apenas:
        
        public StatusAprovacao StatusAprovacao { get; set; } = StatusAprovacao.Pendente;

        public void AprovarUtilizador()
        {
            if (this.StatusAprovacao != StatusAprovacao.Pendente)
            {
                throw new InvalidOperationException($"Não é possível aprovar um utilizador com status {this.StatusAprovacao.ToString()}");
            }
            // TODO: Adicionar Audit Logging
            this.StatusAprovacao = StatusAprovacao.Aprovado;
        }

        public void RejeitarUtilizador()
        {
            if (this.StatusAprovacao != StatusAprovacao.Pendente)
            {
                throw new InvalidOperationException($"Não é possível rejeitar um utilizador com status {this.StatusAprovacao.ToString()}");
            }
            // TODO: Adicionar Audit Logging aqui
            this.StatusAprovacao = StatusAprovacao.Rejeitado;
        }
    }
}
