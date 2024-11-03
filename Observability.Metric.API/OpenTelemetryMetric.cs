using System.Diagnostics.Metrics;

namespace Observability.Metric.API;

public static class OpenTelemetryMetric
{
    private static readonly Meter meter = new Meter("metric.meter.api");
    public static Counter<int> OrderCreatedEventCounter = meter.CreateCounter<int>("order_created_event_count");
    public static ObservableCounter<int> OrderCancelledCounter = meter.CreateObservableCounter
        ("order_cancelled_count", () => new Measurement<int>(Counter.OrderCancelledCounter));

    public static UpDownCounter<int> CurrentStockCount = meter.CreateUpDownCounter<int>("current_stok_counter");
    public static ObservableUpDownCounter<int> CurrentStockObservableCount = meter.CreateObservableUpDownCounter<int>("current_stok_observable_counter", () => new Measurement<int>(Counter.CurrentStockCount));

    public static ObservableGauge<int> rowKitchenTemp = meter.CreateObservableGauge<int>("room_kitchen_template", () => new Measurement<int>(Counter.KitchenTemp));

    public static Histogram<int> MethodDuration = meter.CreateHistogram<int>("method_duration", unit: "miliseconds");
}

public static class Counter
{
    public static int OrderCancelledCounter { get; set; } = 0;
    public static int CurrentStockCount { get; set; } = 0;
    public static int KitchenTemp { get; set; } = 0;
}