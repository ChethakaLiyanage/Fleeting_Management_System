namespace FleetManagement.Application.DTOs.Trips;

public class StartTripDto
{
    public decimal? StartingMileage { get; set; }
    public DateTime? StartTime { get; set; }
    public string? Notes { get; set; }
}
