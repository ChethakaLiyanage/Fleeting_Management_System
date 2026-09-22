using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Inspection : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public InspectionType Type { get; set; } = InspectionType.PreTrip;
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public InspectionResult Result { get; set; } = InspectionResult.Passed;
    public string? Notes { get; set; }

    public ICollection<InspectionItem> Items { get; set; } = new List<InspectionItem>();
}
