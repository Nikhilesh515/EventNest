using System.Text;
using EventNest.AuthService.API.Authorization;
using EventNest.AuthService.API.Middleware;
using EventNest.AuthService.API.SeedData;
using EventNest.AuthService.API.Services;
using EventNest.AuthService.Application.Authorization;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace EventNest.AuthService.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers + Swagger
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        // Authorization — register a policy for every known permission
        services.AddAuthorization(options =>
        {
            var allPermissions = PermissionGroups.All.SelectMany(g => g.Value);
            foreach (var permission in allPermissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        // Authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // gRPC
        services.AddGrpc();

        return services;
    }

    public static async Task MigrateAndSeed(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await context.Database.MigrateAsync();

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await RoleSeedData.SeedAsync(context);
        await AdminSeedData.SeedAsync(context, passwordHasher);
    }

    public static void UseApiMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGrpcService<AuthGrpcService>();
    }
}
