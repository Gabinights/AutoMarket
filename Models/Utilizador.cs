using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models
{
    public class Utilizador : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "A morada não pode exceder 200 caracteres.")]
        public string Morada { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Os contactos não podem exceder 50 caracteres.")]
        public string Contactos { get; set; } = string.Empty;

        public StatusAprovacaoEnum StatusAprovacao { get; set; } = StatusAprovacaoEnum.Pendente;

        public void AprovarUtilizador()
        {
            // TODO: Adicionar Audit Logging aqui
            this.StatusAprovacao = StatusAprovacaoEnum.Aprovado;
        }

        public void RejeitarUtilizador()
        {
            // TODO: Adicionar Audit Logging aqui
            this.StatusAprovacao = StatusAprovacaoEnum.Rejeitado;
        }
    }
}
