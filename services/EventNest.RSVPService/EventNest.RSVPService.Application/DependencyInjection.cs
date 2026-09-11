using EventNest.RSVPService.Application.Services;
using EventNest.RSVPService.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.RSVPService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRsvpService, RsvpService>();
        return services;
    }
}
