using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Maintenance;

public class UpdateMaintenanceStatusRequest
{
    public MaintenanceStatus Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Notes { get; set; }
}
