using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Assignments;

namespace FleetManagement.Application.Interfaces;

public interface IAssignmentService
{
    Task<PagedResult<AssignmentDto>> GetAssignmentsAsync(AssignmentFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<List<AssignmentDto>> GetActiveAssignmentsAsync(CancellationToken cancellationToken = default);
    Task<AssignmentDto> AssignVehicleAsync(CreateAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<AssignmentDto?> EndAssignmentAsync(Guid id, EndAssignmentDto dto, CancellationToken cancellationToken = default);
}
