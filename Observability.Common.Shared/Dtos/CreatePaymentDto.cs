namespace Observability.Common.Shared.Dtos;
public sealed record CreatePaymentDto(
    string OrderCode,
    decimal Total);
