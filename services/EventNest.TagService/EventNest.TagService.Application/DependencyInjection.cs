using EventNest.TagService.Application.Services;
using EventNest.TagService.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventNest.TagService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITagService, Services.TagService>();
        return services;
    }
}
