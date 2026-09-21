using FleetManagement.Application.DTOs.AuditLogs;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class AuditService : IAuditService
{
    private readonly IFleetDbContext _context;

    public AuditService(IFleetDbContext context) => _context = context;

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityType, string entityId)
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(Guid userId)
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task LogAsync(Guid? userId, AuditAction action, string entityType,
        string entityId, string? changes = null, string? ipAddress = null)
    {
        var log = new AuditLog
        {
            UserId     = userId,
            Action     = action,
            EntityType = entityType,
            EntityId   = entityId,
            Changes    = changes,
            IpAddress  = ipAddress
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private static AuditLogDto MapToDto(AuditLog a) => new()
    {
        Id         = a.Id,
        UserId     = a.UserId,
        UserEmail  = a.User?.Email,
        Action     = a.Action.ToString(),
        EntityType = a.EntityType,
        EntityId   = a.EntityId,
        Changes    = a.Changes,
        IpAddress  = a.IpAddress,
        CreatedAt  = a.CreatedAt
    };
}
