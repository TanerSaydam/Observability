using MassTransit;
using MassTransit.Logging;
using Observability.Common.Shared;
using Observability.Common.Shared.Dtos;
using Observability.Logging.Shared;
using Observability.Stock.WebAPI;
using Observability.Stock.WebAPI.Consumers;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog(Logging.ConfigureLogging);
builder.AddOpenTelemetryLog();

builder.Services.AddHttpClient();

builder.Services.AddOpenTelemetry()
    .WithTracing(configure =>
    {
        configure
        .AddSource(DiagnosticHeaders.DefaultListenerName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Stock.WebAPI"))
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = (context) =>
            {
                return context.Request.Path.Value!.Contains("api", StringComparison.OrdinalIgnoreCase);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.EnrichWithHttpRequestMessage = async (activity, request) =>
            {
                string requestContent = string.Empty;
                if (request.Content is not null)
                {
                    requestContent = await request.Content.ReadAsStringAsync();
                }
                activity.SetTag("http.request.body", requestContent);
            };

            options.EnrichWithHttpResponseMessage = async (activity, response) =>
            {
                string responseContent = string.Empty;
                if (response.Content is not null)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }
                activity.SetTag("http.response.body", responseContent);
            };
        })
        .AddConsoleExporter()
        .AddOtlpExporter();
    });

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<StockService>();
builder.Services.AddTransient<RequestAndResponseActivityMiddleware>();
builder.Services.AddTransient<OpenTelemetryTraceIdMiddleware>();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreatedEventConsumer>();

    configure.UsingRabbitMq((context, cfr) =>
    {
        cfr.Host("localhost", "/", c =>
        {
            c.Username("guest");
            c.Password("guest");
        });

        cfr.ReceiveEndpoint("stock.order-created-event.queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/CheckAndPaymentStart", async (StockCheckAndPaymentProcessRequestDto request, StockService stockService, CancellationToken cancellationToken) =>
{
    var response = await stockService.CheckAndPaymentProcessAsync(request);
    if (!response.IsSuccessful)
    {
        return Results.BadRequest(response);
    }

    return Results.Ok(response);
});

app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();

app.Run();
