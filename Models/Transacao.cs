using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums; // Assumindo que os Enums estão aqui

namespace AutoMarket.Models
    {
        public class Transacao
        {
            [Key]
            public int Id { get; set; }

            [Display(Name = "Data da Transação")]
            public DateTime DataTransacao { get; set; } = DateTime.UtcNow;

            [Required]
            [Column(TypeName = "decimal(18,2)")] // Essencial para dinheiro no SQL
            public decimal ValorTotal { get; set; }

            [Required]
            public MetodoPagamento MetodoPagamento { get; set; }

            public EstadoTransacao Estado { get; set; } = EstadoTransacao.Pendente;

            // --- SNAPSHOTS (DADOS IMUTÁVEIS DA FATURA) ---
            // Guardamos estes dados texto para garantir que a fatura não muda
            // mesmo que o user mude o perfil depois.

            [Required]
            [StringLength(200)]
            public string NomeFaturacao { get; set; } = string.Empty;

            [StringLength(20)]
            public string? NifFaturacao { get; set; } // Pode ser null se for Consumidor Final

            [Required]
            [StringLength(300)]
            public string MoradaSnapshot { get; set; } = string.Empty; // Morada + CP

            [StringLength(50)]
            public string? ReferenciaPagamento { get; set; } // Ex: ID do Stripe ou Ref MB

            [StringLength(1000)]
            public string? Observacoes { get; set; }

            // --- RELAÇÕES ---

            [Required]
            public int CarroId { get; set; }

            [ForeignKey("CarroId")]
            public Carro Carro { get; set; }

            // Nota: Relacionamos com o Comprador (perfil de negócio) 
            // e não diretamente com o User (login), mantendo a lógica do sistema.
            [Required]
            public int CompradorId { get; set; }

            [ForeignKey("CompradorId")]
            public Comprador Comprador { get; set; }
        }
    }