using FleetManagement.Application.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Trips;

public class TripFilterParams : PaginationParams
{
    public Guid? VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public TripStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
