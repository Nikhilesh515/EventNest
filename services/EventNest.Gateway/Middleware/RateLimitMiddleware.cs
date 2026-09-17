using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace EventNest.Gateway.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly ConcurrentDictionary<string, ClientBucket> _buckets = new();
    private readonly Timer _cleanupTimer;

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health"
    };

    private const int AnonymousLimit = 100;
    private const int AuthenticatedLimit = 300;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan BucketExpiry = TimeSpan.FromMinutes(5);

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _cleanupTimer = new Timer(CleanupExpiredBuckets, null, BucketExpiry, BucketExpiry);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ExcludedPaths.Contains(context.Request.Path.Value!))
        {
            await _next(context);
            return;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        var limit = isAuthenticated ? AuthenticatedLimit : AnonymousLimit;
        var bucketKey = $"{clientIp}:{(isAuthenticated ? "auth" : "anon")}";

        var bucket = _buckets.GetOrAdd(bucketKey, _ => new ClientBucket(limit, Window));

        using var lease = await bucket.Limiter.AcquireAsync(permitCount: 1, context.RequestAborted);

        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = bucket.GetRemaining().ToString();
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(Window).ToUnixTimeSeconds().ToString();

        if (lease.IsAcquired)
        {
            bucket.LastAccessed = DateTimeOffset.UtcNow;
            bucket.IncrementUsed();
            await _next(context);
        }
        else
        {
            _logger.LogWarning("Rate limit exceeded for {ClientIp} ({AuthStatus})", clientIp, isAuthenticated ? "authenticated" : "anonymous");

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = ((int)Window.TotalSeconds).ToString();
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

    private void CleanupExpiredBuckets(object? state)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(BucketExpiry);
        var expiredKeys = _buckets.Where(kvp => kvp.Value.LastAccessed < cutoff).Select(kvp => kvp.Key).ToList();

        foreach (var key in expiredKeys)
        {
            if (_buckets.TryRemove(key, out var bucket))
            {
                bucket.Limiter.Dispose();
            }
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired rate-limit buckets", expiredKeys.Count);
        }
    }

    private class ClientBucket
    {
        public SlidingWindowRateLimiter Limiter { get; }
        public DateTimeOffset LastAccessed { get; set; }
        public int Limit { get; }
        public int Used { get; private set; }

        public ClientBucket(int limit, TimeSpan window)
        {
            Limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = window,
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
            LastAccessed = DateTimeOffset.UtcNow;
            Limit = limit;
        }

        public void IncrementUsed()
        {
            Used++;
        }

        public int GetRemaining()
        {
            return Math.Max(0, Limit - Used);
        }
    }
}
