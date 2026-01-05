using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.Entities
{
    public class Visita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public Veiculo Veiculo { get; set; } = null!;

        [Required]
        public int CompradorId { get; set; }

        [ForeignKey(nameof(CompradorId))]
        public Comprador Comprador { get; set; } = null!;

        [Required]
        public int VendedorId { get; set; }

        [ForeignKey(nameof(VendedorId))]
        public Vendedor Vendedor { get; set; } = null!;

        [Required]
        public DateTime DataHoraVisita { get; set; }

        [MaxLength(1000)]
        public string? Observacoes { get; set; }

        [Required]
        public EstadoVisita Estado { get; set; } = EstadoVisita.Pendente;
    }
}