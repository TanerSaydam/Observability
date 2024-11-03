using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Observability.Logging.Shared;
public sealed class OpenTelemetryTraceIdMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<OpenTelemetryTraceIdMiddleware>>();
        using (logger.BeginScope("{@traceId}", Activity.Current?.TraceId.ToString()))
        {
            await next(context);
        }
    }
}
