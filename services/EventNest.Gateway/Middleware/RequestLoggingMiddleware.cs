using System.Diagnostics;

namespace EventNest.Gateway.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Request-Id"] = requestId;

        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "[{RequestId}] {Method} {Path} from {RemoteIp}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        context.Response.OnCompleted(() =>
        {
            sw.Stop();
            _logger.LogInformation(
                "[{RequestId}] Response {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
