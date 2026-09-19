using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Vehicles;

namespace FleetManagement.Application.Interfaces;

public interface IVehicleService
{
    Task<PagedResult<VehicleDto>> GetVehiclesAsync(VehicleFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<VehicleDto?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleSummaryDto?> GetVehicleSummaryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto, CancellationToken cancellationToken = default);
    Task<VehicleDto?> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto, CancellationToken cancellationToken = default);
    Task<bool> ArchiveVehicleAsync(Guid id, CancellationToken cancellationToken = default);
}
