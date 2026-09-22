using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Incidents;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize(Roles = "Admin,Driver")]
[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentsController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<IncidentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncidents([FromQuery] IncidentFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _incidentService.GetIncidentsAsync(filterParams, cancellationToken);
        return Ok(ApiResponse<PagedResult<IncidentDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIncidentById(Guid id, CancellationToken cancellationToken)
    {
        var incident = await _incidentService.GetIncidentByIdAsync(id, cancellationToken);
        if (incident == null)
        {
            return NotFound(ApiResponse<IncidentDto>.Fail($"Incident with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<IncidentDto>.Ok(incident));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentDto dto, CancellationToken cancellationToken)
    {
        var incident = await _incidentService.CreateIncidentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetIncidentById), new { id = incident.Id }, ApiResponse<IncidentDto>.Ok(incident, "Incident recorded successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] UpdateIncidentDto dto, CancellationToken cancellationToken)
    {
        var incident = await _incidentService.UpdateIncidentAsync(id, dto, cancellationToken);
        if (incident == null)
        {
            return NotFound(ApiResponse<IncidentDto>.Fail($"Incident with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<IncidentDto>.Ok(incident, "Incident updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteIncident(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _incidentService.DeleteIncidentAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(ApiResponse<bool>.Fail($"Incident with ID '{id}' was not found."));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIncidentStatus(Guid id, [FromBody] UpdateIncidentStatusDto dto, CancellationToken cancellationToken)
    {
        var incident = await _incidentService.UpdateIncidentStatusAsync(id, dto, cancellationToken);
        if (incident == null)
        {
            return NotFound(ApiResponse<IncidentDto>.Fail($"Incident with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<IncidentDto>.Ok(incident, "Incident status updated successfully."));
    }
}
