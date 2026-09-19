using FleetManagement.Application.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Incidents;

public class IncidentFilterParams : PaginationParams
{
    public Guid? VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public IncidentType? Type { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public IncidentStatus? Status { get; set; }
}
