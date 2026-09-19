using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Incidents;

public class UpdateIncidentStatusDto
{
    public IncidentStatus Status { get; set; }
    public decimal? ActualRepairCost { get; set; }
}
