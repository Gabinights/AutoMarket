using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models
{
    /// <summary>
    /// Modelo que representa um veículo no marketplace.
    /// </summary>
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Matrícula única do veículo (ex: 12-AB-34).
        /// </summary>
        [Required(ErrorMessage = "A matrícula é obrigatória.")]
        [StringLength(15, ErrorMessage = "A matrícula não pode exceder 15 caracteres.")]
        public string Matricula { get; set; } = string.Empty;

        /// <summary>
        /// ID do proprietário/vendedor.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Vendedor))]
        public string VendedorId { get; set; } = string.Empty;

        [Required]
        public virtual Utilizador Vendedor { get; set; } = null!;

        /// <summary>
        /// Marca do veículo (ex: BMW, Renault).
        /// </summary>
        [Required(ErrorMessage = "A marca é obrigatória.")]
        [StringLength(50, ErrorMessage = "A marca não pode exceder 50 caracteres.")]
        public string Marca { get; set; } = string.Empty;

        /// <summary>
        /// Modelo do veículo (ex: Série 1, Clio).
        /// </summary>
        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [StringLength(50, ErrorMessage = "O modelo não pode exceder 50 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        /// <summary>
        /// Versão/variante do modelo (opcional).
        /// </summary>
        [StringLength(50, ErrorMessage = "A versão não pode exceder 50 caracteres.")]
        public string? Versao { get; set; }

        /// <summary>
        /// Cor do veículo.
        /// </summary>
        [StringLength(30, ErrorMessage = "A cor não pode exceder 30 caracteres.")]
        public string? Cor { get; set; }

        /// <summary>
        /// Categoria/tipo de veículo (ex: SUV, Sedan, Hatchback).
        /// </summary>
        [StringLength(50, ErrorMessage = "A categoria não pode exceder 50 caracteres.")]
        public string? Categoria { get; set; }

        /// <summary>
        /// Ano de fabrico.
        /// </summary>
        [Range(1900, 2100, ErrorMessage = "O ano deve estar entre 1900 e 2100.")]
        public int? Ano { get; set; }

        /// <summary>
        /// Quilometragem atual.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        public int? Quilometros { get; set; }

        /// <summary>
        /// Tipo de combustível (ex: Gasolina, Gasóleo, Híbrido, Elétrico).
        /// </summary>
        [StringLength(30, ErrorMessage = "O combustível não pode exceder 30 caracteres.")]
        public string? Combustivel { get; set; }

        /// <summary>
        /// Tipo de caixa de velocidades (Manual, Automática).
        /// </summary>
        [StringLength(30, ErrorMessage = "A caixa não pode exceder 30 caracteres.")]
        public string? Caixa { get; set; }

        /// <summary>
        /// Potência do motor em cavalos (cv).
        /// </summary>
        [Range(0, 1000, ErrorMessage = "A potência deve estar entre 0 e 1000 cv.")]
        public int? Potencia { get; set; }

        /// <summary>
        /// Número de portas.
        /// </summary>
        [Range(0, 10, ErrorMessage = "O número de portas deve estar entre 0 e 10.")]
        public int? Portas { get; set; }

        /// <summary>
        /// Preço de venda em euros.
        /// </summary>
        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.01, 999999.99, ErrorMessage = "O preço deve estar entre 0.01€ e 999999.99€.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        /// <summary>
        /// Condição do veículo (Novo, Usado).
        /// </summary>
        [StringLength(20, ErrorMessage = "A condição não pode exceder 20 caracteres.")]
        public string? Condicao { get; set; }

        /// <summary>
        /// Descrição detalhada do veículo.
        /// </summary>
        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        public string? Descricao { get; set; }

        /// <summary>
        /// URL da imagem principal do veículo (apenas nome do ficheiro).
        /// </summary>
        [StringLength(255, ErrorMessage = "A imagem não pode exceder 255 caracteres.")]
        public string? ImagemPrincipal { get; set; }

        /// <summary>
        /// Estado do veículo no sistema.
        /// </summary>
        public EstadoVeiculo Estado { get; set; } = EstadoVeiculo.Ativo;

        /// <summary>
        /// Data e hora de criação.
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data e hora da última modificação.
        /// </summary>
        public DateTime? DataModificacao { get; set; }

        /// <summary>
        /// Data e hora da remoção (soft delete).
        /// </summary>
        public DateTime? DataRemocao { get; set; }
    }
}
