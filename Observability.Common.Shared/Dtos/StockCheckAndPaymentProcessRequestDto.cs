namespace Observability.Common.Shared.Dtos;
public sealed record StockCheckAndPaymentProcessRequestDto(
    string OrderCode,
    List<CreateOrderItemDto> OrderItems
    );