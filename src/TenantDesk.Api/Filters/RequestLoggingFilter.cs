using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace TenantDesk.Api.Filters;

public class RequestLoggingFilter(ILogger<RequestLoggingFilter> logger) : IAsyncActionFilter
{
    private readonly ILogger<RequestLoggingFilter> _logger = logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;

        _logger.LogInformation("Starting request: {Method} {Path}", method, path);

        var stopwatch = Stopwatch.StartNew();

        await next();

        stopwatch.Stop();

        var statusCode = httpContext.Response.StatusCode;
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "Finished request: {Method} {Path} - {StatusCode} - {ElapsedMilliseconds}ms",
            method,
            path,
            statusCode,
            elapsedMs);
    }
}
