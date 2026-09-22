using FleetManagement.Application.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Vehicles;

public class VehicleFilterParams : PaginationParams
{
    public VehicleStatus? Status { get; set; }
    public VehicleType? VehicleType { get; set; }
    public FuelType? FuelType { get; set; }
    public string? Make { get; set; }
}
