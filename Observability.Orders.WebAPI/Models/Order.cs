using Microsoft.EntityFrameworkCore;

namespace Observability.Orders.WebAPI.Models;

public sealed class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = default!;
    public DateTime Created { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem>? Items { get; set; }
}

public enum OrderStatus : byte
{
    Success = 1,
    Fail = 0
}

public sealed class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Count { get; set; }
    [Precision(18, 2)]
    public decimal Price { get; set; }
}