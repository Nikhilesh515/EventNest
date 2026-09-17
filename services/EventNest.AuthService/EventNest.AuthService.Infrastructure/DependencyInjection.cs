using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Options;
using EventNest.AuthService.Infrastructure.Data;
using EventNest.AuthService.Infrastructure.Data.Repositories;
using EventNest.AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuthDb")));

        // Options
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));
        services.Configure<AuthOptions>(opts =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (allowedOrigins is not null)
                opts.AllowedOrigins = allowedOrigins;

            var cookieSecure = configuration.GetValue<bool?>("Cookie:Secure");
            if (cookieSecure.HasValue)
                opts.CookieSecure = cookieSecure.Value;
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPermissionStore, PermissionStore>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        // Cache — Redis optional, falls back to in-memory
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
