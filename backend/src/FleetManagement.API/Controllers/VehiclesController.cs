using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Vehicles;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VehicleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicles([FromQuery] VehicleFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _vehicleService.GetVehiclesAsync(filterParams, cancellationToken);
        return Ok(ApiResponse<PagedResult<VehicleDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleById(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.GetVehicleByIdAsync(id, cancellationToken);
        if (vehicle == null)
        {
            return NotFound(ApiResponse<VehicleDto>.Fail($"Vehicle with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<VehicleDto>.Ok(vehicle));
    }

    [HttpGet("{id:guid}/summary")]
    [ProducesResponseType(typeof(ApiResponse<VehicleSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleSummary(Guid id, CancellationToken cancellationToken)
    {
        var summary = await _vehicleService.GetVehicleSummaryAsync(id, cancellationToken);
        if (summary == null)
        {
            return NotFound(ApiResponse<VehicleSummaryDto>.Fail($"Vehicle with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<VehicleSummaryDto>.Ok(summary));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.CreateVehicleAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, ApiResponse<VehicleDto>.Ok(vehicle, "Vehicle created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleDto dto, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.UpdateVehicleAsync(id, dto, cancellationToken);
        if (vehicle == null)
        {
            return NotFound(ApiResponse<VehicleDto>.Fail($"Vehicle with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<VehicleDto>.Ok(vehicle, "Vehicle updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveVehicle(Guid id, CancellationToken cancellationToken)
    {
        var archived = await _vehicleService.ArchiveVehicleAsync(id, cancellationToken);
        if (!archived)
        {
            return NotFound(ApiResponse<bool>.Fail($"Vehicle with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<bool>.Ok(true, "Vehicle archived successfully."));
    }
}
