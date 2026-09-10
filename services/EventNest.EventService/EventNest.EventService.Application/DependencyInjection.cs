using EventNest.EventService.Application.Services;
using EventNest.EventService.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.EventService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, Services.EventService>();
        return services;
    }
}
