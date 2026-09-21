using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService) =>
        _auditService = auditService;

    /// <summary>Get paginated audit logs.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50) =>
        Ok(await _auditService.GetAllAsync(page, pageSize));

    /// <summary>Get audit logs for a specific entity.</summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityType, string entityId) =>
        Ok(await _auditService.GetByEntityAsync(entityType, entityId));

    /// <summary>Get audit logs for a specific user.</summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId) =>
        Ok(await _auditService.GetByUserAsync(userId));
}
