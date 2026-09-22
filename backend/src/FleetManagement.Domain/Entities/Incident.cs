using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Incident : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Location { get; set; } = string.Empty;
    public IncidentType Type { get; set; } = IncidentType.Accident;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public string? PoliceReportNumber { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public decimal? ActualRepairCost { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Reported;
}
