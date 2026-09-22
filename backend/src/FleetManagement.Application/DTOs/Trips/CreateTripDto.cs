namespace FleetManagement.Application.DTOs.Trips;

public class CreateTripDto
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime? ScheduledStartTime { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
