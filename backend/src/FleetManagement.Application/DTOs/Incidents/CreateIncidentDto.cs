using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Incidents;

public class CreateIncidentDto
{
    public Guid VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? TripId { get; set; }
    public DateTime? Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public IncidentType Type { get; set; } = IncidentType.Accident;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public string? PoliceReportNumber { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public decimal? ActualRepairCost { get; set; }
}
