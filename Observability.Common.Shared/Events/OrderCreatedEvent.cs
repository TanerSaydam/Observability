namespace Observability.Common.Shared.Events;
public sealed class OrderCreatedEvent
{
    public string OrderCode { get; set; } = default!;
}
