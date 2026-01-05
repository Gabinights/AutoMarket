namespace AutoMarket.Models.Entities
{
    public class CarrinhoItem
    {
        public int VeiculoId { get; set; }
        public string Titulo { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public decimal Preco { get; set; }
        public string? ImagemCapa { get; set; }
    }
}