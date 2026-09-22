using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Drivers;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DriverDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDrivers([FromQuery] DriverFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _driverService.GetDriversAsync(filterParams, cancellationToken);
        return Ok(ApiResponse<PagedResult<DriverDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DriverDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriverById(Guid id, CancellationToken cancellationToken)
    {
        var driver = await _driverService.GetDriverByIdAsync(id, cancellationToken);
        if (driver == null)
        {
            return NotFound(ApiResponse<DriverDto>.Fail($"Driver with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<DriverDto>.Ok(driver));
    }

    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<DriverHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriverHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _driverService.GetDriverHistoryAsync(id, cancellationToken);
        if (history == null)
        {
            return NotFound(ApiResponse<DriverHistoryDto>.Fail($"Driver with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<DriverHistoryDto>.Ok(history));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,FleetManager")]
    [ProducesResponseType(typeof(ApiResponse<DriverDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto, CancellationToken cancellationToken)
    {
        var driver = await _driverService.CreateDriverAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, ApiResponse<DriverDto>.Ok(driver, "Driver created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,FleetManager")]
    [ProducesResponseType(typeof(ApiResponse<DriverDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] UpdateDriverDto dto, CancellationToken cancellationToken)
    {
        var driver = await _driverService.UpdateDriverAsync(id, dto, cancellationToken);
        if (driver == null)
        {
            return NotFound(ApiResponse<DriverDto>.Fail($"Driver with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<DriverDto>.Ok(driver, "Driver updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,FleetManager")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateDriver(Guid id, CancellationToken cancellationToken)
    {
        var deactivated = await _driverService.DeactivateDriverAsync(id, cancellationToken);
        if (!deactivated)
        {
            return NotFound(ApiResponse<bool>.Fail($"Driver with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<bool>.Ok(true, "Driver deactivated successfully."));
    }
}
