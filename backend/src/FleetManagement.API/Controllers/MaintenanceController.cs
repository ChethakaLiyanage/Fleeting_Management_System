using FleetManagement.Application.DTOs.Maintenance;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService) =>
        _maintenanceService = maintenanceService;

    /// <summary>Get all maintenance records.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _maintenanceService.GetAllAsync());

    /// <summary>Get maintenance records for a specific vehicle.</summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId) =>
        Ok(await _maintenanceService.GetByVehicleAsync(vehicleId));

    /// <summary>Get all overdue maintenance records.</summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue() =>
        Ok(await _maintenanceService.GetOverdueAsync());

    /// <summary>Get a single maintenance record.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _maintenanceService.GetByIdAsync(id));

    /// <summary>Schedule new maintenance.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateMaintenanceRequest request)
    {
        var result = await _maintenanceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMaintenanceRequest request)
        => Ok(await _maintenanceService.UpdateAsync(id, request));

    /// <summary>Update maintenance status (e.g. complete, cancel).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateMaintenanceStatusRequest request) =>
        Ok(await _maintenanceService.UpdateStatusAsync(id, request));

    /// <summary>Soft-delete a maintenance record.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _maintenanceService.DeleteAsync(id);
        return NoContent();
    }
}
