using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Incidents;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

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
        try
        {
            var incident = await _incidentService.CreateIncidentAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetIncidentById), new { id = incident.Id }, ApiResponse<IncidentDto>.Ok(incident, "Incident recorded successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IncidentDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] UpdateIncidentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var incident = await _incidentService.UpdateIncidentAsync(id, dto, cancellationToken);
            if (incident == null)
            {
                return NotFound(ApiResponse<IncidentDto>.Fail($"Incident with ID '{id}' was not found."));
            }
            return Ok(ApiResponse<IncidentDto>.Ok(incident, "Incident updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IncidentDto>.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:guid}/status")]
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
