using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.ViewModels.Carros
{
    /// <summary>
    /// ViewModel para listar carros (GET - Index).
    /// Apenas propriedades de leitura necessárias para a listagem.
    /// </summary>
    public class ListCarroViewModel
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "Preço")]
        public decimal Preco { get; set; }

        [Display(Name = "Quilómetros")]
        public int Km { get; set; }

        [Display(Name = "Ano")]
        public int Ano { get; set; }

        [Display(Name = "Publicado em")]
        public DateTime DataCriacao { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;

        [Display(Name = "Imagem Principal")]
        public string? ImagemPrincipal { get; set; }

        [Display(Name = "Combustível")]
        public string Combustivel { get; set; } = string.Empty;

        [Display(Name = "Categoria")]
        public string? Categoria { get; set; }

        /// <summary>
        /// Cria um ViewModel a partir de um Carro existente.
        /// </summary>
        public static ListCarroViewModel FromCarro(Carro carro)
        {
            return new ListCarroViewModel
            {
                Id = carro.Id,
                Titulo = carro.Titulo,
                Marca = carro.Marca,
                Modelo = carro.Modelo,
                Preco = carro.Preco,
                Km = carro.Km,
                Ano = carro.Ano,
                DataCriacao = carro.DataCriacao,
                Estado = carro.Estado.ToString(),
                ImagemPrincipal = carro.Imagens?.FirstOrDefault(i => i.IsCapa)?.CaminhoFicheiro,
                Combustivel = carro.Combustivel,
                Categoria = carro.Categoria?.Nome
            };
        }
    }
}
