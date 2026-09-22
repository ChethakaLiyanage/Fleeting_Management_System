using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Assignments;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments([FromQuery] AssignmentFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetAssignmentsAsync(filterParams, cancellationToken);
        return Ok(ApiResponse<PagedResult<AssignmentDto>>.Ok(result));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<AssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAssignments(CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetActiveAssignmentsAsync(cancellationToken);
        return Ok(ApiResponse<List<AssignmentDto>>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignVehicle([FromBody] CreateAssignmentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await _assignmentService.AssignVehicleAsync(dto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<AssignmentDto>.Ok(assignment, "Vehicle assigned successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AssignmentDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/end")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EndAssignment(Guid id, [FromBody] EndAssignmentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await _assignmentService.EndAssignmentAsync(id, dto, cancellationToken);
            if (assignment == null)
            {
                return NotFound(ApiResponse<AssignmentDto>.Fail($"Assignment with ID '{id}' was not found."));
            }
            return Ok(ApiResponse<AssignmentDto>.Ok(assignment, "Assignment ended successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AssignmentDto>.Fail(ex.Message));
        }
    }
}
