using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Inspections;

public class CreateInspectionDto
{
    public Guid VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? TripId { get; set; }
    public InspectionType Type { get; set; } = InspectionType.PreTrip;
    public DateTime? InspectionDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateInspectionItemDto> Items { get; set; } = new();
}

public class CreateInspectionItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public InspectionItemStatus Status { get; set; } = InspectionItemStatus.Pass;
    public string? Notes { get; set; }
}
