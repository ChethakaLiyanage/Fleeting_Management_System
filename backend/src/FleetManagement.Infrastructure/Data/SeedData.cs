using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Infrastructure.Data;

/// <summary>
/// Seeds required lookup/reference data on startup.
/// Only creates roles. Does NOT create any user accounts or hardcode credentials.
/// First admin account should be created via environment-variable-driven seed
/// or a one-time setup endpoint.
/// </summary>
public static class SeedData
{
    private static readonly (string Name, string Description)[] Roles =
    {
        ("Admin",        "Full system access including user and role management."),
        ("FleetManager", "Manage vehicles, drivers, trips, maintenance, and expenses."),
        ("Driver",       "Access own trips, submit fuel records, and report incidents."),
        ("Viewer",       "Read-only access to fleet data.")
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope  = services.CreateAsyncScope();
        var context            = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        var logger             = scope.ServiceProvider.GetRequiredService<ILogger<FleetDbContext>>();

        try
        {
            await SeedRolesAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    private static async Task SeedRolesAsync(FleetDbContext context, ILogger logger)
    {
        foreach (var (name, description) in Roles)
        {
            var exists = await context.Roles.AnyAsync(r => r.Name == name);
            if (!exists)
            {
                context.Roles.Add(new Role { Name = name, Description = description });
                logger.LogInformation("Seeded role: {Role}", name);
            }
        }

        await context.SaveChangesAsync();
    }
}
