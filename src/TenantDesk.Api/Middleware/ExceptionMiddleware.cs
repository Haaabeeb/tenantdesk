namespace TenantDesk.Api.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            await context.Response.WriteAsJsonAsync(new
            {
                error = ex.Message
            });
        }
    }
}
