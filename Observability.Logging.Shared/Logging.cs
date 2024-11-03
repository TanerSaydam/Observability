using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;

namespace Observability.Logging.Shared;
public static class Logging
{

    public static void AddOpenTelemetryLog(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(cfg =>
        {
            cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetSection("OpenTelemetry")["ServiceName"], serviceVersion: "1.0.0"));
            cfg.AddOtlpExporter();
        });
    }
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging => (builderContext, loggerConfiguration) =>
    {
        var environment = builderContext.HostingEnvironment;

        loggerConfiguration
        .ReadFrom.Configuration(builderContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Env", environment.EnvironmentName)
        .Enrich.WithProperty("AppName", environment.ApplicationName);

        var baseUrl = builderContext.Configuration.GetSection("ElasticSearch")["BaseUrl"];
        var userName = builderContext.Configuration.GetSection("ElasticSearch")["UserName"];
        var password = builderContext.Configuration.GetSection("ElasticSearch")["Password"];
        var indexName = builderContext.Configuration.GetSection("ElasticSearch")["IndexName"];

        loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(baseUrl!))
        {
            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
            IndexFormat = $"{indexName}-{environment.EnvironmentName}-logs-" + "{0:yyy.MM.dd}",
            ModifyConnectionSettings = x => x.BasicAuthentication(userName, password),
            CustomFormatter = new ElasticsearchJsonFormatter()
        });
    };
}
