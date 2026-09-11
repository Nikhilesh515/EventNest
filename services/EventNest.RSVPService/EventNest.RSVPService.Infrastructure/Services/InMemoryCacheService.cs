using System.Text.Json;
using EventNest.RSVPService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EventNest.RSVPService.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheService(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out string? json);
        return Task.FromResult(json is null ? default : JsonSerializer.Deserialize<T>(json));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
        };
        _cache.Set(key, json, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
