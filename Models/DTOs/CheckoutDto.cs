using AutoMarket.Models.Enums;

namespace AutoMarket.Models.DTOs
{
    public sealed record CheckoutInitDto(
        string? NomeCompleto,
        string? Morada,
        string? Nif,
        decimal ValorTotal);

    public sealed record CheckoutInputDto(
        string Morada,
        string CodigoPostal,
        bool QueroFaturaComNif,
        string? NifFaturacao,
        MetodoPagamento MetodoPagamento);

    public sealed record CheckoutProcessResultDto(bool Success, int? FirstTransactionId, IEnumerable<string> Errors);

    public sealed record TransacaoDto(
        int Id,
        decimal ValorPago,
        EstadoTransacao Estado,
        DateTime Data,
        string? NifSnapshot,
        string MoradaSnapshot);
}