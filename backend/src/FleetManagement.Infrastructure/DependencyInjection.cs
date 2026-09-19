using FleetManagement.Application.Interfaces;
using FleetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<FleetDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IFleetDbContext>(provider => provider.GetRequiredService<FleetDbContext>());

        return services;
    }
}
