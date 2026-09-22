namespace FleetManagement.Application.DTOs.Assignments;

public class CreateAssignmentDto
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public Guid? AssignedByUserId { get; set; }
    public string? Notes { get; set; }
}
