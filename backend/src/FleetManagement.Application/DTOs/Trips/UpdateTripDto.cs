namespace FleetManagement.Application.DTOs.Trips;

public class UpdateTripDto
{
    public string StartLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
