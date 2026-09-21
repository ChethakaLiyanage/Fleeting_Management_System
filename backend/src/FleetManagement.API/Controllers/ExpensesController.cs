using FleetManagement.Application.DTOs.Expenses;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService) =>
        _expenseService = expenseService;

    /// <summary>Get all expenses.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _expenseService.GetAllAsync());

    /// <summary>Get all pending expenses (awaiting approval).</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() =>
        Ok(await _expenseService.GetPendingAsync());

    /// <summary>Get expenses for a specific vehicle.</summary>
    [HttpGet("vehicle/{{vehicleId:guid}}")]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId) =>
        Ok(await _expenseService.GetByVehicleAsync(vehicleId));

    /// <summary>Get a single expense.</summary>
    [HttpGet("{{id:guid}}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _expenseService.GetByIdAsync(id));

    /// <summary>Submit a new expense.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateExpenseRequest request)
    {{
        var result = await _expenseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new {{ id = result.Id }}, result);
    }}

    /// <summary>Approve a pending expense.</summary>
    [HttpPost("{{id:guid}}/approve")]
    public async Task<IActionResult> Approve(Guid id, ApproveExpenseRequest request) =>
        Ok(await _expenseService.ApproveAsync(id, request));

    /// <summary>Reject a pending expense.</summary>
    [HttpPost("{{id:guid}}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejectExpenseRequest request) =>
        Ok(await _expenseService.RejectAsync(id, request));

    /// <summary>Soft-delete an expense.</summary>
    [HttpDelete("{{id:guid}}")]
    public async Task<IActionResult> Delete(Guid id)
    {{
        await _expenseService.DeleteAsync(id);
        return NoContent();
    }}
}}
