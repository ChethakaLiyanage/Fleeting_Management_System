using FleetManagement.Application.DTOs.Trips;
using FleetManagement.Application.Services;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Xunit;

namespace FleetManagement.UnitTests;

public class TripLifecycleTests
{
    [Fact]
    public async Task StartTrip_ShouldSetVehicleAndDriverStatusToOnTrip()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "TRP-V1", EngineNumber = "ENG-T1", Make = "Toyota", Model = "HiAce", Mileage = 10000, Status = VehicleStatus.Available };
        var driver = new Driver { EmployeeNumber = "TRP-D1", FirstName = "John", LastName = "Doe", LicenseNumber = "TLIC-01", LicenseClass = "A", LicenseIssueDate = DateTime.UtcNow.AddYears(-1), LicenseExpiry = DateTime.UtcNow.AddYears(2), Status = DriverStatus.Available };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var tripService = new TripService(context);
        var trip = await tripService.CreateTripAsync(new CreateTripDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            StartLocation = "Warehouse A",
            Destination = "Distribution Hub B",
            Purpose = "Cargo Delivery"
        });

        var started = await tripService.StartTripAsync(trip.Id, new StartTripDto { StartingMileage = 10000 });

        Assert.Equal(TripStatus.InProgress, started.Status);
        Assert.Equal(VehicleStatus.OnTrip, vehicle.Status);
        Assert.Equal(DriverStatus.OnTrip, driver.Status);
    }

    [Fact]
    public async Task CompleteTrip_WhenEndingMileageLessThanStartingMileage_ShouldThrow()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "TRP-V2", EngineNumber = "ENG-T2", Make = "Ford", Model = "Ranger", Mileage = 5000, Status = VehicleStatus.Available };
        var driver = new Driver { EmployeeNumber = "TRP-D2", FirstName = "Jack", LastName = "Ryan", LicenseNumber = "TLIC-02", LicenseClass = "A", LicenseIssueDate = DateTime.UtcNow.AddYears(-1), LicenseExpiry = DateTime.UtcNow.AddYears(2), Status = DriverStatus.Available };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var tripService = new TripService(context);
        var trip = await tripService.CreateTripAsync(new CreateTripDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            StartLocation = "Colombo",
            Destination = "Kandy",
            Purpose = "Inspection Delivery"
        });

        await tripService.StartTripAsync(trip.Id, new StartTripDto { StartingMileage = 5000 });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tripService.CompleteTripAsync(trip.Id, new CompleteTripDto { EndingMileage = 4900 }));

        Assert.Contains("lower than starting mileage", ex.Message);
    }

    [Fact]
    public async Task CompleteTrip_ShouldCalculateDistance_AndUpdateVehicleMileage()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "TRP-V3", EngineNumber = "ENG-T3", Make = "Isuzu", Model = "D-Max", Mileage = 20000, Status = VehicleStatus.Available };
        var driver = new Driver { EmployeeNumber = "TRP-D3", FirstName = "Mark", LastName = "Twain", LicenseNumber = "TLIC-03", LicenseClass = "A", LicenseIssueDate = DateTime.UtcNow.AddYears(-1), LicenseExpiry = DateTime.UtcNow.AddYears(2), Status = DriverStatus.Available };

        context.Vehicles.Add(vehicle);
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        var tripService = new TripService(context);
        var trip = await tripService.CreateTripAsync(new CreateTripDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            StartLocation = "Site 1",
            Destination = "Site 2",
            Purpose = "Equipment Transfer"
        });

        await tripService.StartTripAsync(trip.Id, new StartTripDto { StartingMileage = 20000 });
        var completed = await tripService.CompleteTripAsync(trip.Id, new CompleteTripDto { EndingMileage = 20150 });

        Assert.Equal(TripStatus.Completed, completed.Status);
        Assert.Equal(150, completed.Distance);
        Assert.Equal(20150, vehicle.Mileage);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(DriverStatus.Available, driver.Status);
    }
}
