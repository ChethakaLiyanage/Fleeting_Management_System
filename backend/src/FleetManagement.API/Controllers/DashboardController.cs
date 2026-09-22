using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize(Roles = "Admin,FleetManager")]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    /// <summary>Get fleet summary KPIs (vehicles, drivers, trips, costs, alerts).</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() =>
        Ok(await _dashboardService.GetSummaryAsync());

    /// <summary>Get active alerts (overdue maintenance, expiring insurance/docs/licenses).</summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts() =>
        Ok(await _dashboardService.GetActiveAlertsAsync());
}
