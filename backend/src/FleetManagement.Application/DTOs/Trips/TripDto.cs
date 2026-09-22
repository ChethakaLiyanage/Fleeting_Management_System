using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Trips;

public class TripDto
{
    public Guid Id { get; set; }
    public string TripNumber { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMakeModel { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string StartLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? StartingMileage { get; set; }
    public decimal? EndingMileage { get; set; }
    public decimal? Distance { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public TripStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
