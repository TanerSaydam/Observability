namespace Observability.Common.Shared.Dtos;
public sealed record CreateOrderDto(
    Guid UserId,
    List<CreateOrderItemDto> Items
    );


public sealed record CreateOrderItemDto(
    int ProductId,
    int Count,
    decimal Price);
