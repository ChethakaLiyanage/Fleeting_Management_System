using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class MaintenanceRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;

    public string Description { get; set; } = string.Empty;
    public string? ServiceProvider { get; set; }

    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    /// <summary>Odometer reading at which maintenance was performed (km).</summary>
    public decimal? OdometerReading { get; set; }

    public decimal? Cost { get; set; }

    /// <summary>Next service due at this odometer (km).</summary>
    public decimal? NextServiceOdometer { get; set; }

    /// <summary>Next service due by this date.</summary>
    public DateTime? NextServiceDate { get; set; }

    public string? Notes { get; set; }

    // Business rules
    public bool IsOverdue => Status == MaintenanceStatus.Scheduled && DateTime.UtcNow > ScheduledDate;
    public bool IsCompleted => Status == MaintenanceStatus.Completed;
}
