using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Inspections;

public class UpdateInspectionDto
{
    public Guid VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? TripId { get; set; }
    public InspectionType Type { get; set; }
    public DateTime InspectionDate { get; set; }
    public InspectionResult Result { get; set; }
    public string? Notes { get; set; }
    public List<UpdateInspectionItemDto> Items { get; set; } = new();
}

public class UpdateInspectionItemDto
{
    public Guid? Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public InspectionItemStatus Status { get; set; }
    public string? Notes { get; set; }
}