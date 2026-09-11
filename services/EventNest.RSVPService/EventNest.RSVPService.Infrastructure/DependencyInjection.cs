using EventNest.RSVPService.Application.Interfaces;
using EventNest.RSVPService.Application.Services.Interfaces;
using EventNest.RSVPService.Infrastructure.Data;
using EventNest.RSVPService.Infrastructure.Data.Repositories;
using EventNest.RSVPService.Infrastructure.Grpc;
using EventNest.RSVPService.Infrastructure.Services;
using EventNest.Shared.Infrastructure.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.RSVPService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RsvpDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("RsvpDb")));

        services.AddScoped<IRsvpRepository, RsvpRepository>();

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

        var eventGrpcUrl = configuration["Services:Event:GrpcUrl"]!;
        services.AddGrpcClient<EventService.EventServiceClient>(o =>
        {
            o.Address = new Uri(eventGrpcUrl);
        });
        services.AddScoped<IEventGrpcClient, Grpc.EventGrpcClient>();

        return services;
    }
}
