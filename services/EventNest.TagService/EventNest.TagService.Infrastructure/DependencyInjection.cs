using EventNest.TagService.Application.Services.Interfaces;
using EventNest.TagService.Infrastructure.Data;
using EventNest.TagService.Infrastructure.Data.Repositories;
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

        return services;
    }
}
