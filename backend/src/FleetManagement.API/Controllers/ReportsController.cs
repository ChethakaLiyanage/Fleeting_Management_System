using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) =>
        _reportService = reportService;

    /// <summary>Fleet utilization report for a date range.</summary>
    [HttpGet("utilization")]
    public async Task<IActionResult> GetUtilization(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) =>
        Ok(await _reportService.GetFleetUtilizationAsync(from, to));

    /// <summary>Cost breakdown report (fuel + maintenance + expenses) for a date range.</summary>
    [HttpGet("costs")]
    public async Task<IActionResult> GetCosts(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) =>
        Ok(await _reportService.GetCostReportAsync(from, to));

    /// <summary>Driver performance report (trips, distance, incidents, failed inspections).</summary>
    [HttpGet("driver-performance")]
    public async Task<IActionResult> GetDriverPerformance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) =>
        Ok(await _reportService.GetDriverPerformanceAsync(from, to));
}
