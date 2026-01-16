using System.ComponentModel.DataAnnotations;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.ViewModels
{
    /// <summary>
    /// ViewModel para exibir transações (compras ou vendas) numa lista
    /// Usado pelo TransacaoService.
    /// </summary>
    public class TransacaoListViewModel
    {
        public int TransacaoId { get; set; }

        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime DataTransacao { get; set; }

        [Display(Name = "Valor Pago")]
        [DataType(DataType.Currency)]
        public decimal ValorPago { get; set; }

        [Display(Name = "Estado")]
        public EstadoTransacao Estado { get; set; }

        [Display(Name = "Método de Pagamento")]
        public MetodoPagamento Metodo { get; set; }

        // Dados do Veículo
        public int VeiculoId { get; set; }

        [Display(Name = "Veículo")]
        public string VeiculoTitulo { get; set; } = string.Empty;

        public string VeiculoMarca { get; set; } = string.Empty;
        public string VeiculoModelo { get; set; } = string.Empty;

        [Display(Name = "Imagem")]
        public string? VeiculoImagemCapa { get; set; }

        // Para Compras: Dados do Vendedor
        [Display(Name = "Vendedor")]
        public string? VendedorNome { get; set; }

        // Para Vendas: Dados do Comprador
        [Display(Name = "Comprador")]
        public string? CompradorNome { get; set; }

        // Snapshots para referência
        public string? MoradaEnvioSnapshot { get; set; }
        public string? NifFaturacaoSnapshot { get; set; }
    }
}
