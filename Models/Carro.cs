using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    public enum EstadoCarro
    {
        Ativo = 0, // Visível no site
        Reservado = 1, // Visível mas não comprável
        Vendido = 2, // Histórico
        Pausado = 3 // Escondido pelo vendedor ou pelo admin
    }

    public class Carro
    {
        [Key] // Isto é desnecessário?? Entity auto key attributting to ID
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
        [StringLength(20)]
        [Display(Name = "Combustível")]
        public string Combustivel { get; set; }

        [StringLength(20)]
        [Display(Name = "Caixa")]
        public string Caixa { get; set; }

        [StringLength(100)]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; } // Ex: "Lisboa"

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
        public ICollection<CarroImagem> Imagens { get; set; }
    }
}