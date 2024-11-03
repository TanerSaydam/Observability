using Observability.Metric.API;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenTelemetry()
    .WithMetrics(options =>
{
    options.AddMeter("metric.meter.api");
    options.ConfigureResource(resource =>
    {
        resource.AddService("Metric.API", serviceVersion: "1.0.0");
    });

    options.AddPrometheusExporter();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("api/increase-cancelled-order", () =>
{
    Counter.OrderCancelledCounter += 1;

    return Counter.OrderCancelledCounter;
});

app.MapGet("/api/counter-metric", () =>
{
    OpenTelemetryMetric.OrderCreatedEventCounter.Add(1,
        new KeyValuePair<string, object?>("event", "add"),
        new KeyValuePair<string, object?>("queue name", "event.created.queue")
        );

    return "Hello World!";
});

app.MapGet("/api/update-down-counter-metric", () =>
{
    OpenTelemetryMetric.CurrentStockCount.Add(1);

    return "Hello World!";
});

app.MapGet("/api/update-down-counter-observable-metric", () =>
{
    Counter.CurrentStockCount += 1;

    return "Hello World!";
});

app.MapGet("/api/gauge-observable-metric", () =>
{
    Counter.KitchenTemp = new Random().Next(-30, 60);

    return "Hello World!";
});

app.MapGet("api/histogram-metric", () =>
{
    OpenTelemetryMetric.MethodDuration.Record(new Random().Next(500, 50000));

    return "Hello world!";
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
