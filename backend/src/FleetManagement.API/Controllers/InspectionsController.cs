using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Inspections;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize(Roles = "Admin,FleetManager,Driver")]
[ApiController]
[Route("api/[controller]")]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _inspectionService;

    public InspectionsController(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InspectionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInspections([FromQuery] InspectionFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _inspectionService.GetInspectionsAsync(filterParams, cancellationToken);
        return Ok(ApiResponse<PagedResult<InspectionDto>>.Ok(result));
    }

    [HttpGet("failed")]
    [ProducesResponseType(typeof(ApiResponse<List<InspectionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFailedInspections(CancellationToken cancellationToken)
    {
        var result = await _inspectionService.GetFailedInspectionsAsync(cancellationToken);
        return Ok(ApiResponse<List<InspectionDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InspectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInspectionById(Guid id, CancellationToken cancellationToken)
    {
        var inspection = await _inspectionService.GetInspectionByIdAsync(id, cancellationToken);
        if (inspection == null)
        {
            return NotFound(ApiResponse<InspectionDto>.Fail($"Inspection with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<InspectionDto>.Ok(inspection));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InspectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInspection([FromBody] CreateInspectionDto dto, CancellationToken cancellationToken)
    {
        var inspection = await _inspectionService.CreateInspectionAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetInspectionById), new { id = inspection.Id }, ApiResponse<InspectionDto>.Ok(inspection, "Inspection recorded successfully."));
    }
}
