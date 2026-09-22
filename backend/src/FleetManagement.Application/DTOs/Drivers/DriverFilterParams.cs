using FleetManagement.Application.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Drivers;

public class DriverFilterParams : PaginationParams
{
    public DriverStatus? Status { get; set; }
    public string? LicenseClass { get; set; }
    public bool? LicenseExpiredOnly { get; set; }
}
