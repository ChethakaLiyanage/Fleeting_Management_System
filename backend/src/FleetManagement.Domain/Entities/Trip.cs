using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Trip : BaseEntity
{
    public string TripNumber { get; set; } = string.Empty;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public string StartLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? StartingMileage { get; set; }
    public decimal? EndingMileage { get; set; }
    public decimal? Distance { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public TripStatus Status { get; set; } = TripStatus.Scheduled;
    public string? Notes { get; set; }
}
