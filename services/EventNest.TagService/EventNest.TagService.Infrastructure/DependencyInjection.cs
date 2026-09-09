using EventNest.TagService.Application.Interfaces;
using EventNest.TagService.Application.Services.Interfaces;
using EventNest.TagService.Infrastructure.Data;
using EventNest.TagService.Infrastructure.Data.Repositories;
using EventNest.TagService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.TagService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TagDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TagDb")));

        services.AddScoped<ITagRepository, TagRepository>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        return services;
    }
}
