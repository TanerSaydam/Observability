using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Observability.Common.Shared;
using Observability.Common.Shared.Dtos;
using Observability.Common.Shared.Events;
using Observability.Logging.Shared;
using Observability.Orders.WebAPI;
using Observability.Orders.WebAPI.Context;
using Observability.Orders.WebAPI.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;
using Order = Observability.Orders.WebAPI.Models.Order;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog(Logging.ConfigureLogging);
builder.AddOpenTelemetryLog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer("Data Source=TANER\\SQLEXPRESS;Initial Catalog=ObservabilityOrdersDb;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
    });
});


builder.Services.AddHttpClient();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisService = sp.GetService<RedisService>()!;
    return redisService.GetConnectionMultiplexer;
});

builder.Services.AddSingleton<RedisService>();

builder.Services.AddOpenTelemetry()
    .WithTracing(configure =>
    {
        configure
        .AddSource("Orders.WebAPI")
        .AddSource(DiagnosticHeaders.DefaultListenerName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Orders.WebAPI"))
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = (context) =>
            {
                return context.Request.Path.Value!.Contains("api", StringComparison.OrdinalIgnoreCase);
            };
        })
        .AddConsoleExporter()
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
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
            options.EnrichWithIDbCommand = (activity, dbCommand) =>
            {

            };
        })
        .AddRedisInstrumentation(options =>
        {
            options.SetVerboseDatabaseStatements = true;
        })
        .AddOtlpExporter();
    });

builder.Services.AddTransient<StokService>();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<RequestAndResponseActivityMiddleware>();
builder.Services.AddTransient<OpenTelemetryTraceIdMiddleware>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/create-send-queue", async (CreateOrderDto request, IPublishEndpoint publishEndpoint) =>
{
    Order order = new()
    {
        UserId = request.UserId,
        Items = request.Items.Select(s => new OrderItem
        {
            Price = s.Price,
            ProductId = s.ProductId,
            Count = s.Count,
        }).ToList(),
        OrderCode = Guid.NewGuid().ToString(),
        Status = OrderStatus.Success,
        Created = DateTime.Now,
    };

    await publishEndpoint.Publish(new OrderCreatedEvent() { OrderCode = order.OrderCode });

    return Results.Ok("Sipariþ oluþturuldu");
});

app.MapPost("/api/create", async (CreateOrderDto request, ApplicationDbContext context, StokService stokService, CancellationToken cancellationToken) =>
{
    Activity.Current!.SetTag("Asp.Net Core Tag 1", "tag value");

    using var activity = ActivitySourceProvider.Source.StartActivity("Starting activitiy...")!;
    activity.AddEvent(new("Sipariþ süreci baþladý"));
    activity.SetTag("order user id", request.UserId);

    context.Database.BeginTransaction();

    try
    {
        Order order = new()
        {
            UserId = request.UserId,
            Items = request.Items.Select(s => new OrderItem
            {
                Price = s.Price,
                ProductId = s.ProductId,
                Count = s.Count,
            }).ToList(),
            OrderCode = Guid.NewGuid().ToString(),
            Status = OrderStatus.Success,
            Created = DateTime.Now,
        };


        context.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        Activity.Current!.SetBaggage("OrderCode", order.OrderCode);


        StockCheckAndPaymentProcessRequestDto stockCheckAndPaymentProcessRequestDto = new(
            order.OrderCode, request.Items);

        var response = await stokService.CheckStockAndStartPaymentAsync(stockCheckAndPaymentProcessRequestDto, cancellationToken);

        if (!response.IsSuccessful)
        {
            context.Database.RollbackTransaction();
            return Results.BadRequest(response);
        }

        context.Database.CommitTransaction();

        activity.AddEvent(new("Sipariþ süreci bitti"));
        return Results.Ok(new { Message = "Sipariþ baþarýyla oluþturuldu" });
    }
    catch (Exception)
    {
        context.Database.RollbackTransaction();
        throw;
    }
});

app.MapGet("/api/getall", async (ApplicationDbContext context, RedisService redisService, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    var orders = new List<Order>();

    var redisDb = redisService.GetDb(0);

    var redisValue = redisDb.StringGet("orders");

    if (redisValue.HasValue)
    {
        orders = JsonSerializer.Deserialize<List<Order>>(redisValue!);
    }
    else
    {
        orders = await context.Orders.Include(p => p.Items).ToListAsync(cancellationToken);
        redisDb.StringSet("orders", JsonSerializer.Serialize(orders), TimeSpan.FromMinutes(10));
    }

    logger.LogInformation("Sipariþ listesi getirildi.{@userId}", Guid.NewGuid());

    return Results.Ok(orders);
});

app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();

app.Run();
