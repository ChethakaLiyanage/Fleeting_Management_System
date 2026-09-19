using FleetManagement.Application.DTOs.Inspections;
using FleetManagement.Application.Services;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Xunit;

namespace FleetManagement.UnitTests;

public class InspectionRuleTests
{
    [Fact]
    public async Task CreateInspection_WithFailedItem_ShouldSetResultFailed_AndVehicleToMaintenance()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "INSP-V1", EngineNumber = "ENG-I1", Make = "Volvo", Model = "FH16", Status = VehicleStatus.Available };
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new InspectionService(context);
        var inspection = await service.CreateInspectionAsync(new CreateInspectionDto
        {
            VehicleId = vehicle.Id,
            Type = InspectionType.PreTrip,
            Items = new List<CreateInspectionItemDto>
            {
                new() { ItemName = "Tyres", Status = InspectionItemStatus.Pass },
                new() { ItemName = "Brakes", Status = InspectionItemStatus.Fail, Notes = "Brake pad worn out" },
                new() { ItemName = "Lights", Status = InspectionItemStatus.Pass }
            }
        });

        Assert.Equal(InspectionResult.Failed, inspection.Result);
        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);
    }

    [Fact]
    public async Task CreateInspection_WithAttentionItemOnly_ShouldSetResultNeedsAttention()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "INSP-V2", EngineNumber = "ENG-I2", Make = "Scania", Model = "R500", Status = VehicleStatus.Available };
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new InspectionService(context);
        var inspection = await service.CreateInspectionAsync(new CreateInspectionDto
        {
            VehicleId = vehicle.Id,
            Type = InspectionType.PreTrip,
            Items = new List<CreateInspectionItemDto>
            {
                new() { ItemName = "Fluids", Status = InspectionItemStatus.Attention, Notes = "Coolant slightly low" },
                new() { ItemName = "Battery", Status = InspectionItemStatus.Pass }
            }
        });

        Assert.Equal(InspectionResult.NeedsAttention, inspection.Result);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
    }

    [Fact]
    public async Task CreateInspection_AllPass_ShouldSetResultPassed()
    {
        using var context = TestDbContextFactory.Create();
        var vehicle = new Vehicle { RegistrationNumber = "INSP-V3", EngineNumber = "ENG-I3", Make = "Mercedes", Model = "Actros", Status = VehicleStatus.Available };
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new InspectionService(context);
        var inspection = await service.CreateInspectionAsync(new CreateInspectionDto
        {
            VehicleId = vehicle.Id,
            Type = InspectionType.PostTrip,
            Items = new List<CreateInspectionItemDto>
            {
                new() { ItemName = "Tyres", Status = InspectionItemStatus.Pass },
                new() { ItemName = "Brakes", Status = InspectionItemStatus.Pass },
                new() { ItemName = "Lights", Status = InspectionItemStatus.Pass }
            }
        });

        Assert.Equal(InspectionResult.Passed, inspection.Result);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
    }
}
