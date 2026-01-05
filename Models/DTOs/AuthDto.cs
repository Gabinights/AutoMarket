using AutoMarket.Models.Enums;

namespace AutoMarket.Models.DTOs
{
    public sealed record RegisterDto(string Email, string Password, string Nome, string Morada, string? Contacto, string? Nif, TipoConta TipoConta);

    public sealed record RegisterResultDto(bool Success, IEnumerable<string> Errors, string? ConfirmationLink);

    public sealed record LoginDto(string Email, string Password, bool RememberMe);

    public sealed record LoginResultDto(bool Success, string? FailureReason, IEnumerable<string> Errors);

    public sealed record ConfirmEmailResultDto(bool Success, IEnumerable<string> Errors);

    public sealed record UpdateNifResultDto(bool Success, IEnumerable<string> Errors);

    public sealed record SoftDeleteResultDto(bool Success, IEnumerable<string> Errors);
}