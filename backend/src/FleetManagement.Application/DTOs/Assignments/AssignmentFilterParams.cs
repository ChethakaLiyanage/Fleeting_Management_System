using FleetManagement.Application.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Assignments;

public class AssignmentFilterParams : PaginationParams
{
    public Guid? VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public AssignmentStatus? Status { get; set; }
}
