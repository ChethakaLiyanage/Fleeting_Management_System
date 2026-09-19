using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Incidents;

public class IncidentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMakeModel { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid? TripId { get; set; }
    public string? TripNumber { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public IncidentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string? PoliceReportNumber { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public decimal? ActualRepairCost { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
