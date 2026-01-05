using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Modelo genérico para Veículos (Carros, Motas, etc.)
    /// Representa um veículo listado para venda no catálogo.
    /// </summary>
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        // --- Dados do veículo ---
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres.")]
        [Display(Name = "Título do Anúncio")]
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
        [Range(1900, 2100, ErrorMessage = "Ano inválido.")]
        public int Ano { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        [Range(0, 99999999, ErrorMessage = "Valor inválido.")]
        public decimal Preco { get; set; }

        [Display(Name = "Quilómetros")]
        [Range(0, int.MaxValue, ErrorMessage = "Valor não pode ser negativo.")]
        public int Km { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Combustível")]
        public string Combustivel { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Caixa")]
        public string Caixa { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; }

        [StringLength(50)]
        [Display(Name = "Condição")]
        public string? Condicao { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "A descrição é demasiado longa.")]
        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; }

        // --- Estado e Controlo ---
        [Display(Name = "Estado")]
        public EstadoVeiculo Estado { get; set; } = EstadoVeiculo.Ativo;

        [Display(Name = "Publicado em")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // --- Relação com Vendedor (1 Vendedor -> N Veículos) ---
        [Required]
        public int VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; }

        // --- Relação com Imagens (1 Veículo -> N Imagens) ---
        public ICollection<VeiculoImagem> Imagens { get; set; } = [];
    }
}
