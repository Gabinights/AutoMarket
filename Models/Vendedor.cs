using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    public enum StatusAprovacao
    {
        Pendente = 0,
        Aprovado = 1,
        Rejeitado = 2
    }

    public class Vendedor
    {
        [Key] //o Identity já define propriedades com "Id" no nome como Primary Key mas senior developers preferem explicitar na mesma caso o nome da propriedade seja alterado e serve de documentação
        public int Id { get; set; }

        //Relação 1:1 com Utilizador
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Utilizador User { get; set; }

        // -- Dados de Negócio -- //
        [StringLength(9, MinimumLength = 9, ErrorMessage = "O NIF deve ter exatamente 9 caracteres.")] // performance (coluna na BD é criada com o número certo de dígitos)
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O NIF deve conter apenas 9 números")]
        public string? NIF { get; set; } = string.Empty;

        //Dados específicos do Vendedor
        [Display(Name = "É Empresa?")]
        public bool IsEmpresa { get; set; } //Indica se o vendedor é uma empresa (=true) ou um particular (=false)

        //Gestão de Aprovação - usado no backoffice admin  
        public StatusAprovacao Status { get; set; } = StatusAprovacao.Pendente;

        //Registo de quem aprovou (auditoria)
        public string? ApprovedByAdminId { get; set; } //ID do admin que aprovou o vendedor

        [StringLength(500)] 
        public string? MotivoRejeicao { get; set; } //Motivo da rejeição, se aplicável

        //Lista de carros que este vendedor tem à venda
        public ICollection<Carro> CarrosAVenda { get; set; }

        // -- Métodos de Domínio (Lógica de negócio) --

        public void Aprovar(string adminId)
        {
            if (this.Status != StatusAprovacao.Pendente)
                throw new InvalidOperationException($"Apenas vendedores pendentes podem ser aprovados. Estado atual: {this.Status}");

            this.Status = StatusAprovacao.Aprovado;
            this.ApprovedByAdminId = adminId;
            this.MotivoRejeicao = null; // Limpa o motivo de rejeição se houver
        }

        public void Rejeitar(string adminId, string motivo)
        {
            if (this.Status != StatusAprovacao.Pendente)
                throw new InvalidOperationException($"Apenas vendedores pendentes podem ser rejeitados.");

            this.Status = StatusAprovacao.Rejeitado;
            this.ApprovedByAdminId = adminId;
            this.MotivoRejeicao = motivo; // Define o motivo da rejeição 
        }

        public void Ressubmeter()
        {
            if(this.Status == StatusAprovacao.Rejeitado)
            {
                this.Status = StatusAprovacao.Pendente;
            }
        }
    }
}

