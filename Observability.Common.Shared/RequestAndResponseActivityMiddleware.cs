using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System.Diagnostics;

namespace Observability.Common.Shared;
public sealed class RequestAndResponseActivityMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await AddRequestBodyContentToActivityTags(context);
        await AddResponseBodyContentToActivityTags(context, next);
    }

    private async Task AddRequestBodyContentToActivityTags(HttpContext context)
    {
        context.Request.EnableBuffering();
        var requestBodyStreamReader = new StreamReader(context.Request.Body);
        var requestBody = await requestBodyStreamReader.ReadToEndAsync();
        Activity.Current!.SetTag("http.request.body", requestBody);
        context.Request.Body.Position = 0;
    }

    private async Task AddResponseBodyContentToActivityTags(HttpContext context, RequestDelegate next)
    {
        var originalResponse = context.Response.Body;

        RecyclableMemoryStreamManager recyclableMemoryStream = new();

        await using var responseBodyMemoryStream = recyclableMemoryStream.GetStream();
        context.Response.Body = responseBodyMemoryStream;

        await next(context);

        responseBodyMemoryStream.Position = 0;

        var responseBodyStreamReader = new StreamReader(responseBodyMemoryStream);
        var responseBody = await responseBodyStreamReader.ReadToEndAsync();

        Activity.Current!.SetTag("http.response.body", responseBody);
        context.Response.Body.Position = 0;

        await responseBodyMemoryStream.CopyToAsync(originalResponse);
    }
}