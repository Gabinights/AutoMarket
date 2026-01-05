using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Regista todas as ações administrativas para auditoria.
    /// Imutável após criação (histórico permanente).
    /// </summary>
    public class AuditoriaLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificação do administrador que realizou a ação.
        /// </summary>
        [Required]
        public string AdminId { get; set; } = string.Empty;

        [ForeignKey("AdminId")]
        public Utilizador? Admin { get; set; }

        /// <summary>
        /// Tipo de ação: "VENDEDOR_APROVADO", "USUARIO_BLOQUEADO", "DENUNCIA_ENCERRADA", etc.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TipoAcao { get; set; } = string.Empty;

        /// <summary>
        /// Descrição legível da ação realizada.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// ID da entidade afetada (VendedorId, UtilizadorId, DenunciaId, etc.)
        /// </summary>
        [StringLength(450)]
        public string? EntidadeAfetadaId { get; set; }

        /// <summary>
        /// Tipo da entidade: "Utilizador", "Vendedor", "Denuncia", "Veiculo", etc.
        /// </summary>
        [StringLength(100)]
        public string? TipoEntidade { get; set; }

        /// <summary>
        /// Dados antes da mudança (JSON serializado para auditoria).
        /// </summary>
        public string? DadosAntigos { get; set; }

        /// <summary>
        /// Dados após a mudança (JSON serializado).
        /// </summary>
        public string? DadosNovos { get; set; }

        /// <summary>
        /// Endereço IP da requisição.
        /// </summary>
        [StringLength(50)]
        public string? EnderecoIP { get; set; }

        /// <summary>
        /// User-Agent do navegador.
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Timestamp UTC da ação.
        /// </summary>
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
    }
}
