using System.Threading.RateLimiting;

namespace EventNest.Gateway.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SlidingWindowRateLimiter _limiter;
    private readonly ILogger<RateLimitMiddleware> _logger;

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health"
    };

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ExcludedPaths.Contains(context.Request.Path.Value!))
        {
            await _next(context);
            return;
        }

        using var lease = await _limiter.AcquireAsync(permitCount: 1, context.RequestAborted);

        if (lease.IsAcquired)
        {
            await _next(context);
        }
        else
        {
            _logger.LogWarning("Rate limit exceeded for {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers["Content-Type"] = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = 429,
                    message = "Rate limit exceeded. Please try again later."
                }
            });
        }
    }
}
