using System.Net;

namespace EventNest.Gateway.Middleware;

public class GatewayExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayExceptionHandlerMiddleware> _logger;

    public GatewayExceptionHandlerMiddleware(RequestDelegate next, ILogger<GatewayExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway error: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            TimeoutException => (HttpStatusCode.GatewayTimeout, "Service timeout"),
            HttpRequestException => (HttpStatusCode.BadGateway, "Service unavailable"),
            TaskCanceledException => (HttpStatusCode.GatewayTimeout, "Request timeout"),
            OperationCanceledException => (HttpStatusCode.GatewayTimeout, "Request cancelled"),
            _ => (HttpStatusCode.InternalServerError, "Internal gateway error")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = new
            {
                code = (int)statusCode,
                message,
                detail = exception.Message
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
