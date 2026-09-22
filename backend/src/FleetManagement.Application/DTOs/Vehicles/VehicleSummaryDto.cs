using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Vehicles;

public class VehicleSummaryDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; }
    public decimal CurrentMileage { get; set; }
    public DateTime? RegistrationExpiry { get; set; }
    public bool IsRegistrationExpiringSoon { get; set; }
    public int TotalTrips { get; set; }
    public int TotalInspections { get; set; }
    public int TotalIncidents { get; set; }
}
