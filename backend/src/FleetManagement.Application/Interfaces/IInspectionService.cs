using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Inspections;

namespace FleetManagement.Application.Interfaces;

public interface IInspectionService
{
    Task<PagedResult<InspectionDto>> GetInspectionsAsync(InspectionFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<InspectionDto?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<InspectionDto>> GetFailedInspectionsAsync(CancellationToken cancellationToken = default);
    Task<InspectionDto> CreateInspectionAsync(CreateInspectionDto dto, CancellationToken cancellationToken = default);
    Task<InspectionDto?> UpdateInspectionAsync(Guid id, UpdateInspectionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteInspectionAsync(Guid id, CancellationToken cancellationToken = default);
}
