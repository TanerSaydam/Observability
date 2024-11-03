using Observability.Common.Shared;
using Observability.Common.Shared.Dtos;
using Observability.Logging.Shared;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using TS.Result;


var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog(Logging.ConfigureLogging);
builder.AddOpenTelemetryLog();

builder.Services.AddTransient<RequestAndResponseActivityMiddleware>();

builder.Services.AddOpenTelemetry()
    .WithTracing(configure =>
    {
        configure
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Payments.API"))
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter()
        .AddHttpClientInstrumentation();
    });

builder.Services.AddTransient<OpenTelemetryTraceIdMiddleware>();

var app = builder.Build();

app.MapPost("/api/pay", (CreatePaymentDto request) =>
{
    var orderCode = Activity.Current!.GetBaggageItem("OrderCode");
    decimal balance = 1000;

    if (request.Total > balance)
    {
        return Results.BadRequest(Result<string>.Failure("Yetersiz bakiye"));
    }

    return Results.Ok(Result<string>.Succeed("Ödeme baþarýlý"));
});

app.Use(async (context, next) =>
{
    await next(context);
});

app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();

app.Run();
