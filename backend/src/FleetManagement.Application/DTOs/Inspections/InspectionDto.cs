using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Inspections;

public class InspectionDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMakeModel { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid? TripId { get; set; }
    public string? TripNumber { get; set; }
    public InspectionType Type { get; set; }
    public DateTime InspectionDate { get; set; }
    public InspectionResult Result { get; set; }
    public string? Notes { get; set; }
    public List<InspectionItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class InspectionItemDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public InspectionItemStatus Status { get; set; }
    public string? Notes { get; set; }
}
