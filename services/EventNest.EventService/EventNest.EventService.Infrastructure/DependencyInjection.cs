using EventNest.EventService.Application.Interfaces;
using EventNest.EventService.Application.Services.Interfaces;
using EventNest.EventService.Infrastructure.Data;
using EventNest.EventService.Infrastructure.Data.Repositories;
using EventNest.EventService.Infrastructure.Grpc;
using EventNest.EventService.Infrastructure.Services;
using EventNest.Shared.Infrastructure.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.EventService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EventDb")));

        services.AddScoped<IEventRepository, EventRepository>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        var authGrpcUrl = configuration["Services:Auth:GrpcUrl"]!;
        services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
        {
            o.Address = new Uri(authGrpcUrl);
        });
        services.AddScoped<IUserGrpcClient, AuthGrpcClient>();

        var tagGrpcUrl = configuration["Services:Tag:GrpcUrl"]!;
        services.AddGrpcClient<TagService.TagServiceClient>(o =>
        {
            o.Address = new Uri(tagGrpcUrl);
        });
        services.AddScoped<ITagGrpcClient, TagGrpcClient>();

        return services;
    }
}
