using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Representa uma visita agendada a um veículo.
    /// 
    /// Workflow:
    /// 1. Comprador agenda visita ? Visita criada (Pendente)
    /// 2. Vendedor pode confirmar ? Estado = Confirmada
    /// 3. Após data/hora ? Estado = Realizada ou NaoRealizada
    /// 4. Pode ser cancelada ? Estado = Cancelada
    /// 
    /// Validações:
    /// - Data/hora não pode ser no passado
    /// - Não é possível agendar visita para veículo vendido
    /// - Não é possível agendar múltiplas visitas no mesmo horário (opcional)
    /// </summary>
    public class Visita
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Veículo que está a ser visitado</summary>
        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo Veiculo { get; set; } = null!;

        /// <summary>Comprador que agendou a visita</summary>
        [Required]
        public int CompradorId { get; set; }

        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        /// <summary>Vendedor do veículo (desnormalizado para facilitar queries)</summary>
        [Required]
        public int VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;

        /// <summary>Data e hora agendada para a visita</summary>
        [Required]
        [Display(Name = "Data/Hora da Visita")]
        public DateTime DataHora { get; set; }

        /// <summary>Quando a visita foi agendada</summary>
        [Display(Name = "Data de Agendamento")]
        public DateTime DataAgendamento { get; set; } = DateTime.UtcNow;

        /// <summary>Estado actual da visita</summary>
        [Display(Name = "Estado")]
        public EstadoVisita Estado { get; set; } = EstadoVisita.Pendente;

        /// <summary>Motivo de cancelamento (se aplicável)</summary>
        [StringLength(500)]
        public string? MotivoCancel { get; set; }

        /// <summary>Notas do comprador sobre a visita</summary>
        [StringLength(500)]
        public string? Notas { get; set; }

        /// <summary>Notas do vendedor sobre a visita</summary>
        [StringLength(500)]
        public string? NotasVendedor { get; set; }

        /// <summary>
        /// Verifica se a data/hora da visita já passou.
        /// </summary>
        public bool DataJáPassou => DateTime.UtcNow > DataHora;

        /// <summary>
        /// Verifica se a visita está agendada para o futuro (válida).
        /// </summary>
        public bool EstáAgendada => DataHora > DateTime.UtcNow && Estado != EstadoVisita.Cancelada;
    }
}
