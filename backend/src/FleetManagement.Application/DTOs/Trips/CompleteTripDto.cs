namespace FleetManagement.Application.DTOs.Trips;

public class CompleteTripDto
{
    public decimal EndingMileage { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Notes { get; set; }
}
