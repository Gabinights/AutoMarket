using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models.Entities
{
    public class Reserva
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
        public DateTime DataReserva { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DataExpiracao { get; set; }

        [Required]
        public EstadoReserva Estado { get; set; } = EstadoReserva.Ativa;
    }
}