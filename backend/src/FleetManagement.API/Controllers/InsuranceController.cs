using FleetManagement.Application.DTOs.Insurance;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsuranceController : ControllerBase
{{
    private readonly IInsuranceService _insuranceService;

    public InsuranceController(IInsuranceService insuranceService) =>
        _insuranceService = insuranceService;

    /// <summary>Get all insurance policies.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _insuranceService.GetAllAsync());

    /// <summary>Get insurance policies expiring within 30 days.</summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring() =>
        Ok(await _insuranceService.GetExpiringAsync());

    /// <summary>Get insurance policies for a specific vehicle.</summary>
    [HttpGet("vehicle/{{vehicleId:guid}}")]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId) =>
        Ok(await _insuranceService.GetByVehicleAsync(vehicleId));

    /// <summary>Get a single insurance policy.</summary>
    [HttpGet("{{id:guid}}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _insuranceService.GetByIdAsync(id));

    /// <summary>Create a new insurance policy.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateInsurancePolicyRequest request)
    {{
        var result = await _insuranceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new {{ id = result.Id }}, result);
    }}

    /// <summary>Soft-delete an insurance policy.</summary>
    [HttpDelete("{{id:guid}}")]
    public async Task<IActionResult> Delete(Guid id)
    {{
        await _insuranceService.DeleteAsync(id);
        return NoContent();
    }}
}}
