using System.ComponentModel.DataAnnotations;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.ViewModels.Veiculos
{
    /// <summary>
    /// ViewModel para criar um novo veículo (POST).
    /// Mapeia para Veiculo durante a criação.
    /// </summary>
    public class CreateVeiculoViewModel
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(100, ErrorMessage = "O título não pode ter mais de 100 caracteres")]
        [Display(Name = "Título do Anúncio")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A marca é obrigatória")]
        [StringLength(50, ErrorMessage = "A marca não pode ter mais de 50 caracteres")]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório")]
        [StringLength(50, ErrorMessage = "O modelo não pode ter mais de 50 caracteres")]
        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Tipo/Segmento")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "O ano é obrigatório")]
        [Range(1900, 2100, ErrorMessage = "O ano deve estar entre 1900 e 2100")]
        [Display(Name = "Ano")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 9999999.99, ErrorMessage = "O preço deve ser superior a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Preço (€)")]
        public decimal Preco { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa")]
        [Display(Name = "Quilómetros")]
        public int Km { get; set; }

        [Required(ErrorMessage = "O combustível é obrigatório")]
        [StringLength(20, ErrorMessage = "O combustível não pode ter mais de 20 caracteres")]
        [Display(Name = "Combustível")]
        public string Combustivel { get; set; } = string.Empty;

        [Required(ErrorMessage = "A caixa é obrigatória")]
        [StringLength(20, ErrorMessage = "A caixa não pode ter mais de 20 caracteres")]
        [Display(Name = "Caixa")]
        public string Caixa { get; set; } = string.Empty;

        [Required(ErrorMessage = "A localização é obrigatória")]
        [StringLength(100, ErrorMessage = "A localização não pode ter mais de 100 caracteres")]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "A condição não pode ter mais de 50 caracteres")]
        [Display(Name = "Condição")]
        public string? Condicao { get; set; }

        [Display(Name = "Fotos")]
        public List<IFormFile> Fotos { get; set; } = new();

        // Propriedades auxiliares para a View
        public List<Categoria> CategoriesDropdown { get; set; } = new();

        /// <summary>
        /// Converte este ViewModel para uma Model Veiculo.
        /// </summary>
        public Veiculo ToVeiculo(int vendedorId)
        {
            return new Veiculo
            {
                Titulo = Titulo,
                Marca = Marca,
                Modelo = Modelo,
                Ano = Ano,
                CategoriaId = CategoriaId,
                Combustivel = Combustivel,
                Caixa = Caixa,
                Km = Km,
                Preco = Preco,
                Condicao = Condicao,
                Localizacao = Localizacao,
                Descricao = Descricao,
                VendedorId = vendedorId,
                DataCriacao = DateTime.UtcNow,
                Estado = EstadoVeiculo.Ativo
            };
        }
    }
}
