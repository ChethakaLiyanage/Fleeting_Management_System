using FleetManagement.Application.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Inspections;

public class InspectionFilterParams : PaginationParams
{
    public Guid? VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public InspectionType? Type { get; set; }
    public InspectionResult? Result { get; set; }
}
