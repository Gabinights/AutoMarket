namespace AutoMarket.Models.DTOs
{
    public sealed record VeiculoDto(int Id, string Marca, string Modelo, int Ano, decimal Preco, EstadoVeiculo Estado, bool IsDeleted);

    // Keep for API if needed/ Remove if replacing with ViewModels 
    public sealed record VeiculoCreateDto(string Marca, string Modelo, int Ano, decimal Preco, EstadoVeiculo Estado);
    public sealed record VeiculoUpdateDto(string Marca, string Modelo, int Ano, decimal Preco, EstadoVeiculo Estado);
    public sealed record VeiculoFiltroDto(string? Marca, string? Modelo, int? AnoMin, int? AnoMax, decimal? PrecoMin, decimal? PrecoMax);
    public sealed record ValidationResultDto(bool IsValid, IEnumerable<string> Errors);
    public sealed record CommandResultDto(bool Success, IEnumerable<string> Errors);

    public class VeiculoFilterOptionsDto
    {
        public List<string> Marcas { get; set; } = new List<string>();
        public List<string> Modelos { get; set; } = new List<string>();
        public List<int> Anos { get; set; } = new List<int>();
        public List<string> Combustiveis { get; set; } = new List<string>();
        public List<string> Caixas { get; set; } = new List<string>();
        public List<string> Categorias { get; set; } = new List<string>();
    }
}