using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Representa uma reserva de veículo feita por um comprador.
    /// 
    /// Workflow:
    /// 1. Comprador clica em "Reservar" ? Reserva criada (Pendente)
    /// 2. Sistema define DataExpiracao (ex: 7 dias)
    /// 3. Se expirar ? Estado = Expirada
    /// 4. Se comprador confirma ? Estado = Confirmada
    /// 5. Se comprador compra ? Estado = Concluida + cria Transacao
    /// 6. Se cancelado ? Estado = Cancelada
    /// 
    /// Durante a reserva, o veículo passa para estado "Reservado" (bloqueado temporariamente).
    /// </summary>
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Veículo que está a ser reservado</summary>
        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo Veiculo { get; set; } = null!;

        /// <summary>Comprador que fez a reserva</summary>
        [Required]
        public int CompradorId { get; set; }

        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        /// <summary>Quando a reserva foi criada</summary>
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        /// <summary>Até quando a reserva é válida</summary>
        [Display(Name = "Data de Expiração")]
        public DateTime DataExpiracao { get; set; }

        /// <summary>Estado actual da reserva</summary>
        [Display(Name = "Estado")]
        public EstadoReserva Estado { get; set; } = EstadoReserva.Pendente;

        /// <summary>Razão do cancelamento (se aplicável)</summary>
        [StringLength(500)]
        public string? MotivoCancel { get; set; }

        /// <summary>Notas do comprador na reserva</summary>
        [StringLength(500)]
        public string? Notas { get; set; }

        /// <summary>
        /// Verifica se a reserva expirou.
        /// Útil para validação antes de operações.
        /// </summary>
        public bool EstáExpirada => DateTime.UtcNow > DataExpiracao && Estado == EstadoReserva.Pendente;

        /// <summary>
        /// Verifica se a reserva ainda é válida (não expirou e não foi cancelada).
        /// </summary>
        public bool EstáVálida => !EstáExpirada && Estado != EstadoReserva.Cancelada && Estado != EstadoReserva.Expirada;
    }
}
