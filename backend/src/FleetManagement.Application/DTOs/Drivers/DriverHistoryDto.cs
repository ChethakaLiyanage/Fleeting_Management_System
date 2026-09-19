using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Drivers;

public class DriverHistoryDto
{
    public Guid DriverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DriverStatus Status { get; set; }
    public int TotalTripsCompleted { get; set; }
    public decimal TotalKilometersDriven { get; set; }
    public int TotalIncidentsInvolved { get; set; }
    public Guid? CurrentlyAssignedVehicleId { get; set; }
    public string? CurrentlyAssignedVehicleReg { get; set; }
}
