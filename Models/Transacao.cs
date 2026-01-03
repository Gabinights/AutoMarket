using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    public enum MetodoPagamento
    {
        CartaoCredito,
        MBWay,
        Transferencia,
        Numerario
    }

    public enum EstadoTransacao
    {
        Pendente,
        Pago,
        Enviado,
        Cancelado
    }

    public class Transacao
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Data da Transação")]
        public DateTime DataTransacao { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Currency)]
        public decimal ValorPago { get; set; }

        public MetodoPagamento Metodo { get; set; }

        // Exemplo: Pendente, Pago, Cancelado
        public EstadoTransacao Estado { get; set; } = EstadoTransacao.Pendente;

        // Snapshot dos dados para fatura (caso o user mude de morada depois)
        public string MoradaEnvioSnapshot { get; set; } = string.Empty;
        public string NifFaturacaoSnapshot { get; set; } = string.Empty;

        // Relações
        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo Veiculo { get; set; }

        [Required]
        public int CompradorId { get; set; }

        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; }
    }
}