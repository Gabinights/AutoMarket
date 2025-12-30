using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models
{
    public class Carro
    {
        [Key] // é boa prática especificar data annotations que por regra o EF já trata (blindagem)
        public int Id { get; set; }

        // --- Dados do veículo ---
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres.")]
        [Display(Name = "Título do Anúncio")]
        public string Titulo { get; set; } // Ex: "Mclaren MP4-4 do Senna"

        [Required]
        [StringLength(50)]
        public string Marca { get; set; } // Ex: Mclaren

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
        public int Ano { get; set; } // Ex: 1991

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        [Range(0, 99999999, ErrorMessage = "Valor inválido.")]
        // A conversão para o tipo 'money' do SQL será feita no DbContext ou via atributo
        public decimal Preco { get; set; } // Ex: 100.000.000

        [Display(Name = "Quilómetros")] //[Display] : Altera o HTML gerado nas views, boa prática DRY (Don't Repeat yourself). Defines o nome bonito num sítio (Model) e todas as 50 Views que usam esse campo ficam atualizadas automaticamente
        [Range(0, int.MaxValue, ErrorMessage = "Valor não pode ser negativo.")] // Restringe a números não-negativos
        public int Km { get; set; }

        // Dropdowns fixos (vamos simplificar como string por agora, ou Enum se preferires rigidez)
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
        public string Localizacao { get; set; } // Ex: "Lisboa"

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "A descrição é demasiado longa.")]
        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; }

        // --- Estado e Controlo ---
        [Display(Name = "Estado")]
        public EstadoCarro Estado { get; set; } = EstadoCarro.Ativo;

        [Display(Name = "Publicado em")]
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