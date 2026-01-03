using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    public class Mensagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Conteudo { get; set; } = string.Empty;

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool Lida { get; set; } = false;

        [Required]
        public string RemetenteId { get; set; } = string.Empty;

        [ForeignKey("RemetenteId")]
        public Utilizador Remetente { get; set; }

        [Required]
        public string DestinatarioId { get; set; } = string.Empty;

        [ForeignKey("DestinatarioId")]
        public Utilizador Destinatario { get; set; }

        // Contexto: Sobre que carro est�o a falar?
        public int? CarroId { get; set; }
        public Carro? Carro { get; set; }
    }
}