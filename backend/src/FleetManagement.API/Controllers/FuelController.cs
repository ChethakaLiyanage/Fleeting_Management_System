using FleetManagement.Application.DTOs.Fuel;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize(Roles = "Admin,FleetManager,Driver")]
[ApiController]
[Route("api/[controller]")]
public class FuelController : ControllerBase
{
    private readonly IFuelService _fuelService;

    public FuelController(IFuelService fuelService) => _fuelService = fuelService;

    /// <summary>Get all fuel records.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _fuelService.GetAllAsync());

    /// <summary>Get all fuel records for a specific vehicle.</summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId) =>
        Ok(await _fuelService.GetByVehicleAsync(vehicleId));

    /// <summary>Get a single fuel record.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _fuelService.GetByIdAsync(id));

    /// <summary>Get fuel efficiency summary for a vehicle.</summary>
    [HttpGet("vehicle/{vehicleId:guid}/efficiency")]
    public async Task<IActionResult> GetEfficiency(Guid vehicleId) =>
        Ok(await _fuelService.GetEfficiencyAsync(vehicleId));

    /// <summary>Log a new fuel record.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateFuelRecordRequest request)
    {
        var result = await _fuelService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Soft-delete a fuel record.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,FleetManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _fuelService.DeleteAsync(id);
        return NoContent();
    }
}
