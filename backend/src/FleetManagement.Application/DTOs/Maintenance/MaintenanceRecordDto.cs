namespace FleetManagement.Application.DTOs.Maintenance;

public class MaintenanceRecordDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistration { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ServiceProvider { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? OdometerReading { get; set; }
    public decimal? Cost { get; set; }
    public decimal? NextServiceOdometer { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public string? Notes { get; set; }
    public bool IsOverdue { get; set; }
}
