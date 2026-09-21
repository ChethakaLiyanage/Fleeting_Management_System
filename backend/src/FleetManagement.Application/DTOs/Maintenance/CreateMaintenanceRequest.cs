using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Maintenance;

public class CreateMaintenanceRequest
{
    public Guid VehicleId { get; set; }
    public MaintenanceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ServiceProvider { get; set; }
    public DateTime ScheduledDate { get; set; }
    public decimal? OdometerReading { get; set; }
    public decimal? Cost { get; set; }
    public decimal? NextServiceOdometer { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public string? Notes { get; set; }
}
