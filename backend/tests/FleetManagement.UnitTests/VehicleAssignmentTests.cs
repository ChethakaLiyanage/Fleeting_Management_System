using FleetManagement.Application.DTOs.Assignments;
using FleetManagement.Application.Services;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Xunit;

namespace FleetManagement.UnitTests;

public class VehicleAssignmentTests
{
    [Fact]
    public async Task AssignVehicleAsync_ShouldSucceed_AndSetStatusesToAssigned()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle
        {
            RegistrationNumber = "CAR-1001",
            EngineNumber = "ENG-001",
            Make = "Toyota",
            Model = "Corolla",
            Status = VehicleStatus.Available
        };
        var driver = new Driver
        {
            EmployeeNumber = "DRV-001",
            FirstName = "Alice",
            LastName = "Smith",
            LicenseNumber = "LIC-001",
            LicenseClass = "Class A",
            LicenseIssueDate = DateTime.UtcNow.AddYears(-2),
            LicenseExpiry = DateTime.UtcNow.AddYears(3),
            Status = DriverStatus.Available
        };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var result = await service.AssignVehicleAsync(new CreateAssignmentDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id
        });

        Assert.Equal(AssignmentStatus.Active, result.Status);
        Assert.Equal(VehicleStatus.Assigned, vehicle.Status);
        Assert.Equal(DriverStatus.Assigned, driver.Status);
    }

    [Fact]
    public async Task AssignVehicleAsync_WhenVehicleUnderMaintenance_ShouldThrowInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle
        {
            RegistrationNumber = "CAR-1002",
            EngineNumber = "ENG-002",
            Make = "Ford",
            Model = "Transit",
            Status = VehicleStatus.Maintenance
        };
        var driver = new Driver
        {
            EmployeeNumber = "DRV-002",
            FirstName = "Bob",
            LastName = "Jones",
            LicenseNumber = "LIC-002",
            LicenseClass = "Class B",
            LicenseIssueDate = DateTime.UtcNow.AddYears(-1),
            LicenseExpiry = DateTime.UtcNow.AddYears(2),
            Status = DriverStatus.Available
        };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignVehicleAsync(new CreateAssignmentDto
            {
                VehicleId = vehicle.Id,
                DriverId = driver.Id
            }));

        Assert.Contains("Maintenance", ex.Message);
    }

    [Fact]
    public async Task AssignVehicleAsync_WhenDriverLicenseExpired_ShouldThrowInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle
        {
            RegistrationNumber = "CAR-1003",
            EngineNumber = "ENG-003",
            Make = "Nissan",
            Model = "NV200",
            Status = VehicleStatus.Available
        };
        var driver = new Driver
        {
            EmployeeNumber = "DRV-003",
            FirstName = "Charlie",
            LastName = "Brown",
            LicenseNumber = "LIC-003",
            LicenseClass = "Class C",
            LicenseIssueDate = DateTime.UtcNow.AddYears(-5),
            LicenseExpiry = DateTime.UtcNow.AddDays(-1), // Expired!
            Status = DriverStatus.Available
        };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignVehicleAsync(new CreateAssignmentDto
            {
                VehicleId = vehicle.Id,
                DriverId = driver.Id
            }));

        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignVehicleAsync_WhenVehicleAlreadyHasActiveAssignment_ShouldThrow()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "CAR-1004", EngineNumber = "ENG-004", Make = "Honda", Model = "Civic" };
        var driver1 = new Driver { EmployeeNumber = "DRV-004", FirstName = "D1", LastName = "L1", LicenseNumber = "L-004", LicenseClass = "A", LicenseIssueDate = DateTime.UtcNow.AddYears(-1), LicenseExpiry = DateTime.UtcNow.AddYears(2) };
        var driver2 = new Driver { EmployeeNumber = "DRV-005", FirstName = "D2", LastName = "L2", LicenseNumber = "L-005", LicenseClass = "A", LicenseIssueDate = DateTime.UtcNow.AddYears(-1), LicenseExpiry = DateTime.UtcNow.AddYears(2) };

        context.Vehicles.Add(vehicle);
        context.Drivers.AddRange(driver1, driver2);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        await service.AssignVehicleAsync(new CreateAssignmentDto { VehicleId = vehicle.Id, DriverId = driver1.Id });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignVehicleAsync(new CreateAssignmentDto { VehicleId = vehicle.Id, DriverId = driver2.Id }));

        Assert.Contains("active assignment", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EndAssignmentAsync_ShouldRestoreStatusesToAvailable()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "CAR-1005", EngineNumber = "ENG-005", Make = "Toyota", Model = "RAV4", Status = VehicleStatus.Available };
        var driver = new Driver { EmployeeNumber = "DRV-006", FirstName = "Eve", LastName = "Green", LicenseNumber = "L-006", LicenseClass = "A", LicenseIssueDate = DateTime.UtcNow.AddYears(-1), LicenseExpiry = DateTime.UtcNow.AddYears(2), Status = DriverStatus.Available };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var assignment = await service.AssignVehicleAsync(new CreateAssignmentDto { VehicleId = vehicle.Id, DriverId = driver.Id });

        var ended = await service.EndAssignmentAsync(assignment.Id, new EndAssignmentDto { Notes = "End of shift" });

        Assert.NotNull(ended);
        Assert.Equal(AssignmentStatus.Ended, ended.Status);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(DriverStatus.Available, driver.Status);
    }
}
