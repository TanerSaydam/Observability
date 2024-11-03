using MassTransit;
using Observability.Common.Shared.Events;
using System.Diagnostics;
using System.Text.Json;

namespace Observability.Stock.WebAPI.Consumers;

public sealed class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        Thread.Sleep(2000);

        var a = Activity.Current;

        Activity.Current?.SetTag("message.body", JsonSerializer.Serialize(context.Message));

        await Task.CompletedTask;
    }
}
