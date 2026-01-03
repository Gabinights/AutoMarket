using AutoMarket.Models.Enums;

namespace AutoMarket.Models.ViewModels.Veiculos
{
    /// <summary>
    /// ViewModel para listar veículos (GET).
    /// Usado para exibir informações resumidas de veículos no catálogo.
    /// </summary>
    public class ListVeiculoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int Km { get; set; }
        public string Combustivel { get; set; }
        public string Caixa { get; set; }
        public string Localizacao { get; set; }
        public string Descricao { get; set; }
        public string? Condicao { get; set; }
        public EstadoVeiculo Estado { get; set; }
        public DateTime DataCriacao { get; set; }
        public int VendedorId { get; set; }
        public string VendedorNome { get; set; }
        public string? ImagemPrincipal { get; set; }
        public string CategoriaNome { get; set; }

        /// <summary>
        /// Converte um Veiculo para ListVeiculoViewModel.
        /// </summary>
        public static ListVeiculoViewModel FromVeiculo(Veiculo veiculo)
        {
            var imagemPrincipal = veiculo.Imagens?.FirstOrDefault(i => i.IsCapa)?.CaminhoFicheiro
                                  ?? veiculo.Imagens?.FirstOrDefault()?.CaminhoFicheiro;

            return new ListVeiculoViewModel
            {
                Id = veiculo.Id,
                Titulo = veiculo.Titulo,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Ano = veiculo.Ano,
                Preco = veiculo.Preco,
                Km = veiculo.Km,
                Combustivel = veiculo.Combustivel,
                Caixa = veiculo.Caixa,
                Localizacao = veiculo.Localizacao,
                Descricao = veiculo.Descricao,
                Condicao = veiculo.Condicao,
                Estado = veiculo.Estado,
                DataCriacao = veiculo.DataCriacao,
                VendedorId = veiculo.VendedorId,
                VendedorNome = veiculo.Vendedor?.User?.Nome ?? "Desconhecido",
                ImagemPrincipal = imagemPrincipal,
                CategoriaNome = veiculo.Categoria?.Nome ?? "Sem categoria"
            };
        }
    }
}
