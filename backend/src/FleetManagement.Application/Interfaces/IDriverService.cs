using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Drivers;

namespace FleetManagement.Application.Interfaces;

public interface IDriverService
{
    Task<PagedResult<DriverDto>> GetDriversAsync(DriverFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<DriverDto?> GetDriverByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DriverHistoryDto?> GetDriverHistoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DriverDto> CreateDriverAsync(CreateDriverDto dto, CancellationToken cancellationToken = default);
    Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateDriverAsync(Guid id, CancellationToken cancellationToken = default);
}
