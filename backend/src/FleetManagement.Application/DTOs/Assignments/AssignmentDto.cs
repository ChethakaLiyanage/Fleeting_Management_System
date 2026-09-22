using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Assignments;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMakeModel { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverEmployeeNumber { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public AssignmentStatus Status { get; set; }
    public Guid? AssignedByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
