using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.Entities
{
    public class Carro
    {
        [Key]
        public int Id { get; set; }

        // --- Dados do veículo ---
        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } // Ex: "Mclaren MP4-4 do Senna"

        [Required]
        [StringLength(50)]
        public string Marca { get; set; } // Ex: Mclaren

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; } // Ex: MP4-4

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        public int Km { get; set; }

        // Dropdowns fixos (vamos simplificar como string por agora, ou Enum se preferires rigidez)
        [Required]
        [StringLength(20)]
        public string Combustivel { get; set; }

        [Required]
        [StringLength(20)]
        public string Caixa { get; set; }

        [Required]
        [StringLength(100)]
        public string Localizacao { get; set; } // Ex: "Lisboa"

        [Required]
        [StringLength(2000)]
        public string Descricao { get; set; }

        // --- Estado e Controlo ---

        public EstadoCarro Estado { get; set; } = EstadoCarro.Ativo;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // --- Relacao com Vendedor (1 Vendedor -> N Carros) ---
        [Required]
        public int VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; }

        // --- Relação com Imagens (1 Carro -> N Imagens) ---
        public ICollection<CarroImagem> Imagens { get; set; } = [];
    }
}