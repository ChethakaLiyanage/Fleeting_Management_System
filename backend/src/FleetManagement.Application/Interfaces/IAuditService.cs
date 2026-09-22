using FleetManagement.Application.DTOs.AuditLogs;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.Interfaces;

public interface IAuditService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityType, string entityId);
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(Guid userId);
    Task LogAsync(Guid? userId, AuditAction action, string entityType, string entityId,
                  string? changes = null, string? ipAddress = null);
}
