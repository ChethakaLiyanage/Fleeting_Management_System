using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FleetManagement.Application.Services;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FleetManagement.Infrastructure.Data;

/// <summary>
/// Seeds required database tables and lookup/reference data on startup.
/// </summary>
public static class SeedData
{
    private static readonly (string Name, string Description)[] Roles =
    {
        ("Admin",        "Full system access including user and role management."),
        ("Driver",       "Access own trips, submit fuel records, and report incidents.")
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope  = services.CreateAsyncScope();
        var context            = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        var logger             = scope.ServiceProvider.GetRequiredService<ILogger<FleetDbContext>>();

        try
        {
            // Test database connection first
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogError("Cannot connect to the database. Please check:");
                logger.LogError("1. PostgreSQL server is running on localhost:5432");
                logger.LogError("2. Database 'fleet_management' exists");
                logger.LogError("3. Username 'postgres' and password are correct");
                logger.LogError("4. Connection string in appsettings.json is correct");
                throw new InvalidOperationException("Database connection failed. See logs for details.");
            }

            // Ensure database is created
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrated/created successfully.");

            await SeedRolesAsync(context, logger);
            await SeedSampleDataAsync(context, logger);
            await EnsureDriverAccountsAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw; // Re-throw to prevent silent failures
        }
    }

    private static async Task SeedRolesAsync(FleetDbContext context, ILogger logger)
    {
        var obsoleteRoles = await context.Roles
            .Where(r => r.Name == "FleetManager" || r.Name == "Viewer")
            .ToListAsync();
        foreach (var role in obsoleteRoles)
        {
            var assignments = await context.UserRoles.Where(ur => ur.RoleId == role.Id).ToListAsync();
            context.UserRoles.RemoveRange(assignments);
            context.Roles.Remove(role);
            logger.LogInformation("Removed obsolete role: {Role}", role.Name);
        }

        foreach (var (name, _) in Roles)
        {
            var exists = await context.Roles.AnyAsync(r => r.Name == name);
            if (!exists)
            {
                context.Roles.Add(new Role { Name = name });
                logger.LogInformation("Seeded role: {Role}", name);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSampleDataAsync(FleetDbContext context, ILogger logger)
    {
        // 1. Seed Admin User
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var passwordService = new PasswordService();
        if (adminRole != null && !await context.Users.AnyAsync(u => u.Email == "admin@fleetos.com"))
        {
            var adminUser = new User
            {
                FirstName    = "System",
                LastName     = "Admin",
                Email        = "admin@fleetos.com",
                PasswordHash = passwordService.Hash("admin123"),
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            };
            adminUser.UserRoles.Add(new UserRole { RoleId = adminRole.Id });
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default admin user: admin@fleetos.com");
        }

        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@fleetos.com");
        if (existingAdmin != null && !existingAdmin.PasswordHash.StartsWith("PBKDF2-SHA256$"))
        {
            existingAdmin.PasswordHash = passwordService.Hash("admin123");
            await context.SaveChangesAsync();
        }

        // 2. Seed Sample Vehicles
        if (!await context.Vehicles.AnyAsync())
        {
            context.Vehicles.AddRange(
                new Vehicle
                {
                    RegistrationNumber = "WP-CAB-1234",
                    Make               = "Toyota",
                    Model              = "HiAce",
                    Year               = 2022,
                    VehicleType        = VehicleType.Van,
                    FuelType           = FuelType.Diesel,
                    Transmission       = TransmissionType.Automatic,
                    Color              = "White",
                    Mileage            = 45200,
                    Status             = VehicleStatus.Available,
                    EngineNumber       = "1KD-FTV-8821",
                    CreatedAt          = DateTime.UtcNow
                },
                new Vehicle
                {
                    RegistrationNumber = "WP-CAA-5678",
                    Make               = "Isuzu",
                    Model              = "NPR",
                    Year               = 2021,
                    VehicleType        = VehicleType.Truck,
                    FuelType           = FuelType.Diesel,
                    Transmission       = TransmissionType.Manual,
                    Color              = "Blue",
                    Mileage            = 89400,
                    Status             = VehicleStatus.Available,
                    EngineNumber       = "4HK1-TCS-4412",
                    CreatedAt          = DateTime.UtcNow
                },
                new Vehicle
                {
                    RegistrationNumber = "WP-CAD-9012",
                    Make               = "Nissan",
                    Model              = "Caravan",
                    Year               = 2023,
                    VehicleType        = VehicleType.Van,
                    FuelType           = FuelType.Petrol,
                    Transmission       = TransmissionType.Automatic,
                    Color              = "Silver",
                    Mileage            = 21350,
                    Status             = VehicleStatus.Available,
                    EngineNumber       = "QR20DE-1903",
                    CreatedAt          = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded initial vehicles.");
        }

        // 3. Seed Sample Drivers
        if (!await context.Drivers.AnyAsync())
        {
            var driverRole = await context.Roles.SingleAsync(r => r.Name == "Driver");
            var seededDrivers = new[]
            {
                new Driver
                {
                    EmployeeNumber    = "DRV-001",
                    FirstName         = "Sunil",
                    LastName          = "Perera",
                    Phone             = "+94 77 123 4567",
                    Email             = "sunil@fleetos.com",
                    Address           = "No. 45, Galle Road, Colombo",
                    LicenseNumber     = "B1234567",
                    LicenseClass      = "Heavy Vehicle",
                    LicenseIssueDate  = DateTime.UtcNow.AddYears(-5),
                    LicenseExpiry     = DateTime.UtcNow.AddYears(3),
                    Status            = DriverStatus.Available,
                    EmergencyContact  = "+94 71 987 6543",
                    CreatedAt         = DateTime.UtcNow
                },
                new Driver
                {
                    EmployeeNumber    = "DRV-002",
                    FirstName         = "Kasun",
                    LastName          = "Silva",
                    Phone             = "+94 71 234 5678",
                    Email             = "kasun@fleetos.com",
                    Address           = "No. 12, Kandy Road, Kelaniya",
                    LicenseNumber     = "B7654321",
                    LicenseClass      = "Light Vehicle",
                    LicenseIssueDate  = DateTime.UtcNow.AddYears(-3),
                    LicenseExpiry     = DateTime.UtcNow.AddYears(2),
                    Status            = DriverStatus.Available,
                    EmergencyContact  = "+94 77 555 4321",
                    CreatedAt         = DateTime.UtcNow
                }
            };
            foreach (var driver in seededDrivers)
            {
                var user = new User
                {
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    Email = driver.Email!,
                    PasswordHash = passwordService.Hash("driver123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.UserRoles.Add(new UserRole { RoleId = driverRole.Id });
                driver.User = user;
                context.Drivers.Add(driver);
            }
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded initial drivers.");
        }
    }

    private static async Task EnsureDriverAccountsAsync(FleetDbContext context, ILogger logger)
    {
        var driverRole = await context.Roles.SingleAsync(r => r.Name == "Driver");
        var drivers = await context.Drivers
            .Include(d => d.User)
            .ToListAsync();

        foreach (var driver in drivers)
        {
            if (driver.User == null)
            {
                throw new InvalidOperationException(
                    $"Driver '{driver.EmployeeNumber}' has no linked user account.");
            }

            if (!await context.UserRoles.AnyAsync(ur =>
                    ur.UserId == driver.UserId && ur.RoleId == driverRole.Id))
            {
                context.UserRoles.Add(new UserRole
                {
                    UserId = driver.UserId,
                    RoleId = driverRole.Id
                });
                logger.LogInformation("Assigned Driver role to user {UserId}.", driver.UserId);
            }
        }

        await context.SaveChangesAsync();
    }
}
