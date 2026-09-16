using EventNest.AuthService.Application.Authorization;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.Shared.Application.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.AuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, Services.AuthService>();
        services.AddScoped<IUserService, Services.UserService>();
        services.AddScoped<IPermissionService, Services.PermissionService>();
        services.AddScoped<IRoleService, Services.RoleService>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        return services;
    }
}
