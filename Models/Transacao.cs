using System.ComponentModel.DataAnnotations;

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
        public DateTime DataTransacao { get; set; } = DateTime.Now;

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
        public int CarroId { get; set; }
        public Carro Carro { get; set; }

        public int CompradorId { get; set; }
        public Comprador Comprador { get; set; }
    }
}