using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(configure =>
    {
        configure
        .AddSource("Observability.WebAPI")
        .ConfigureResource(resourse =>
        {
            resourse.AddService("WebAPI", serviceVersion: "1.0.0");
        })
        //.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebAPI2"))
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = (context) =>
            {
                return context.Request.Path.Value!.Contains("api", StringComparison.OrdinalIgnoreCase);
            };

            options.RecordException = true;
        })
        .AddConsoleExporter()
        .AddOtlpExporter();

    });

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();
