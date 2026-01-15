using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Modelo genérico para Veículos (Carros, Motas, etc.)
    /// Representa um veiculo listado para venda no catálogo.
    /// </summary>
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        // --- Dados do veiculo ---
        [Required(ErrorMessage = "O titulo e obrigatorio")]
        [StringLength(100, ErrorMessage = "Maximo de 100 caracteres.")]
        [Display(Name = "Titulo do Anuncio")]
        public string Titulo { get; set; } // Ex: "McLaren MP4-4 do Senna"

        [Required]
        [StringLength(50)]
        public string Marca { get; set; } // Ex: McLaren

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; } // Ex: MP4-4

        [Required]
        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        [Required]
        [Display(Name = "Ano")]
        [Range(1900, 2100, ErrorMessage = "Ano invalido.")]
        public int Ano { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Preco")]
        [Range(0, 99999999, ErrorMessage = "Valor invalido.")]
        public decimal Preco { get; set; }

        [Display(Name = "Quilometros")]
        [Range(0, int.MaxValue, ErrorMessage = "Valor nao pode ser negativo.")]
        public int Km { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Combustivel")]
        public string Combustivel { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Caixa")]
        public string Caixa { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Localizacao")]
        public string Localizacao { get; set; }

        [StringLength(50)]
        [Display(Name = "Condicao")]
        public string? Condicao { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "A descricao e demasiado longa.")]
        [Display(Name = "Descricao Detalhada")]
        public string Descricao { get; set; }

        // --- Estado e Controlo ---
        [Display(Name = "Estado")]
        public EstadoVeiculo Estado { get; set; } = EstadoVeiculo.Ativo;

        [Display(Name = "Publicado em")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // --- Relacao com Vendedor (1 Vendedor -> N Veiculos) ---
        [Required]
        public int VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; }

        // --- Relacao com Imagens (1 Veiculo -> N Imagens) ---
        public ICollection<VeiculoImagem> Imagens { get; set; } = [];
    }
}
