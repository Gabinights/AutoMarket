using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Notificações in-app para utilizadores.
    /// Regista eventos: denúncias, aprovações, mensagens, etc.
    /// </summary>
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do utilizador que recebe a notificação.
        /// </summary>
        [Required]
        public string DestinatarioId { get; set; } = string.Empty;

        [ForeignKey("DestinatarioId")]
        public Utilizador? Destinatario { get; set; }

        /// <summary>
        /// Tipo de notificação: "DENUNCIA_ABERTA", "VENDEDOR_APROVADO", "RESERVA_CONFIRMADA", etc.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Assunto da notificação (resumo).
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Assunto { get; set; } = string.Empty;

        /// <summary>
        /// Corpo da notificação (descrição detalhada).
        /// </summary>
        [Required]
        public string Corpo { get; set; } = string.Empty;

        /// <summary>
        /// Link para a página relacionada (ex: /Admin/Denuncias/5).
        /// </summary>
        [StringLength(500)]
        public string? LinkRelacionado { get; set; }

        /// <summary>
        /// Se a notificação foi lida.
        /// </summary>
        public bool Lida { get; set; } = false;

        /// <summary>
        /// Timestamp da notificação.
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID da entidade relacionada (ex: DenunciaId, VendedorId).
        /// </summary>
        [StringLength(450)]
        public string? EntidadeRelacionadaId { get; set; }

        /// <summary>
        /// Tipo da entidade relacionada.
        /// </summary>
        [StringLength(100)]
        public string? TipoEntidadeRelacionada { get; set; }
    }
}
