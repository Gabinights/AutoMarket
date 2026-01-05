using AutoMarket.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models.Entities
{
    public class Vendedor
    {
        [Key] //o Identity já define propriedades com "Id" no nome como Primary Key mas senior developers preferem explicitar na mesma caso o nome da propriedade seja alterado e serve de documentação
        public int Id { get; set; }

        //Relação 1:1 com Utilizador
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Utilizador User { get; set; } = null!;

        [Required] // EF já faz por defeito a propriedade enum como NOT NULL na BD mas é boa prática explicitar
        public TipoConta TipoConta { get; set; }

        //Gestão de Aprovação - usado no backoffice admin  
        public StatusAprovacao Status { get; set; } = StatusAprovacao.Pendente;

        //Registo de quem aprovou (auditoria)
        [StringLength(450)]
        public string? ApprovedByAdminId { get; set; } //ID do admin que aprovou o vendedor

        [ForeignKey("ApprovedByAdminId")]
        public Utilizador? ApprovedByAdmin { get; set; } //Navegação para o admin que aprovou o vendedor

        [StringLength(500)] 
        public string? MotivoRejeicao { get; set; } //Motivo da rejeição, se aplicável

        //Lista de carros que este vendedor tem à venda
        public ICollection<Veiculo> VeiculosAVenda { get; set; } = [];

        // -- Métodos de Domínio (Lógica de negócio) --

        public void Aprovar(string adminId)
        {
            if (Status != StatusAprovacao.Pendente)
                throw new InvalidOperationException($"Apenas vendedores pendentes podem ser aprovados. Estado atual: {Status}");

            Status = StatusAprovacao.Aprovado;
            ApprovedByAdminId = adminId;
            MotivoRejeicao = null; // Limpa o motivo de rejeição se houver
        }

        public void Rejeitar(string adminId, string motivo)
        {
            if (Status != StatusAprovacao.Pendente)
                throw new InvalidOperationException($"Apenas vendedores pendentes podem ser rejeitados.");

            Status = StatusAprovacao.Rejeitado;
            ApprovedByAdminId = adminId;
            MotivoRejeicao = motivo; // Define o motivo da rejeição 
        }

        public void Ressubmeter()
        {
            if (Status != StatusAprovacao.Rejeitado)
                throw new InvalidOperationException($"Apenas vendedores rejeitados podem ser ressubmetidos. Estado atual: {Status}");
            
            Status = StatusAprovacao.Pendente;
            MotivoRejeicao = null; // Limpa o motivo anterior
        }
    }
}

