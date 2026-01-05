namespace AutoMarket.Models.DTOs
{
    public sealed record VeiculoCreateDto(string Marca, string Modelo, int Ano, decimal Preco, EstadoVeiculo Estado);
    public sealed record VeiculoUpdateDto(string Marca, string Modelo, int Ano, decimal Preco, EstadoVeiculo Estado);
    public sealed record VeiculoDto(int Id, string Marca, string Modelo, int Ano, decimal Preco, EstadoVeiculo Estado, bool IsDeleted);
    public sealed record VeiculoFiltroDto(string? Marca, string? Modelo, int? AnoMin, int? AnoMax, decimal? PrecoMin, decimal? PrecoMax);
    public sealed record ValidationResultDto(bool IsValid, IEnumerable<string> Errors);
    public sealed record CommandResultDto(bool Success, IEnumerable<string> Errors);
}