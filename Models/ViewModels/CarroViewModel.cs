using System.ComponentModel.DataAnnotations;
using AutoMarket.Models.Enums;

namespace AutoMarket.Models.ViewModels
{
    public class CarroViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Título do Anúncio")]
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres.")]
        public string Titulo { get; set; }

        [Display(Name = "Marca")]
        [Required(ErrorMessage = "A marca é obrigatória.")]
        public string Marca { get; set; }

        [Display(Name = "Modelo")]
        [Required(ErrorMessage = "O modelo é obrigatório.")]
        public string Modelo { get; set; }

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        [Display(Name = "Ano de Fabrico")]
        [Range(1900, 2100, ErrorMessage = "Insira um ano válido.")]
        public int Ano { get; set; }

        [Display(Name = "Preço (€)")]
        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.01, 10000000, ErrorMessage = "O preço deve ser superior a 0.")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [Display(Name = "Quilómetros")]
        [Range(0, int.MaxValue, ErrorMessage = "Os quilómetros não podem ser negativos.")]
        public int Km { get; set; }

        [Display(Name = "Combustível")]
        [Required(ErrorMessage = "Selecione o tipo de combustível.")]
        public string Combustivel { get; set; }

        [Display(Name = "Caixa de Velocidades")]
        [Required(ErrorMessage = "Selecione o tipo de caixa.")]
        public string Caixa { get; set; }

        [Display(Name = "Localização")]
        [Required(ErrorMessage = "A localização é obrigatória.")]
        public string Localizacao { get; set; }

        [Display(Name = "Descrição Detalhada")]
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição é demasiado longa.")]
        [DataType(DataType.MultilineText)]
        public string Descricao { get; set; }

        // Campo auxiliar para Upload (não vai para a BD diretamente)
        [Display(Name = "Fotografias do Veículo")]
        public List<IFormFile>? ImagensUpload { get; set; }

        // Apenas para edição: mostrar imagens existentes
        public ICollection<CarroImagem>? ImagensAtuais { get; set; }
    }
}