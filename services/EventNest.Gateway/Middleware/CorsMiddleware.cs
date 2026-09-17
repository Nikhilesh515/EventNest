using Microsoft.Extensions.Options;

namespace EventNest.Gateway.Middleware;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = [];
}

public class CorsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _allowedOrigins;

    public CorsMiddleware(RequestDelegate next, IOptions<CorsOptions> options)
    {
        _next = next;
        _allowedOrigins = new HashSet<string>(options.Value.AllowedOrigins, StringComparer.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();

        if (!string.IsNullOrEmpty(origin) && _allowedOrigins.Contains(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Request-Id";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
        }

        if (HttpMethods.IsOptions(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            await context.Response.CompleteAsync();
            return;
        }

        await _next(context);
    }
}
