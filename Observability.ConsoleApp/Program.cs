using Observability.ConsoleApp;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

ActivitySource.AddActivityListener(new ActivityListener()
{
    ShouldListenTo = source => source.Name == OpenTelemtryConstants.ActivitySourceFileName,
    ActivityStarted = activity =>
    {
        Console.WriteLine("Activity is started");
    },
    ActivityStopped = activity =>
    {
        Console.WriteLine("Activity is stopped");
    }
});


using var traceProvideFile = Sdk
    .CreateTracerProviderBuilder()
    .AddSource(OpenTelemtryConstants.ActivitySourceFileName)
    .Build();

using TracerProvider traceProvider = Sdk
    .CreateTracerProviderBuilder()
    .AddSource(OpenTelemtryConstants.ActivitySourceName)
    .ConfigureResource(configure =>
    {
        configure
        .AddService(OpenTelemtryConstants.ServiceName, serviceVersion: OpenTelemtryConstants.ServiceVersion)
        .AddAttributes(new List<KeyValuePair<string, object>>()
        {
            new("host.machineName", Environment.MachineName),
            new("host.os",Environment.OSVersion.VersionString),
            new("dotnet.version", Environment.Version.ToString()),
            new("host.environment", "dev")
        });
    })
    .AddConsoleExporter()
    .AddOtlpExporter()
    .AddZipkinExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
    })
    .Build();

var serviceHelper = new ServiceHelper();
await serviceHelper.Work1();

Console.ReadLine();