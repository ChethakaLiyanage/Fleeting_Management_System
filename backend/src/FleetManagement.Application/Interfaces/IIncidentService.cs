using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Incidents;

namespace FleetManagement.Application.Interfaces;

public interface IIncidentService
{
    Task<PagedResult<IncidentDto>> GetIncidentsAsync(IncidentFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<IncidentDto?> GetIncidentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, CancellationToken cancellationToken = default);
    Task<IncidentDto?> UpdateIncidentAsync(Guid id, UpdateIncidentDto dto, CancellationToken cancellationToken = default);
    Task<IncidentDto?> UpdateIncidentStatusAsync(Guid id, UpdateIncidentStatusDto dto, CancellationToken cancellationToken = default);
}
