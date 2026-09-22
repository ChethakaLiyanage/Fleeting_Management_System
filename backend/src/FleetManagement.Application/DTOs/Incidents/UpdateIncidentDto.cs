using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Incidents;

public class UpdateIncidentDto
{
    public DateTime? Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public IncidentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string? PoliceReportNumber { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public decimal? ActualRepairCost { get; set; }
    public IncidentStatus Status { get; set; }
}
