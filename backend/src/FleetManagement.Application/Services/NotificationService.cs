using FleetManagement.Application.DTOs.Notifications;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IFleetDbContext _context;

    public NotificationService(IFleetDbContext context) => _context = context;

    public async Task<IEnumerable<NotificationDto>> GetByUserAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            UserId        = request.UserId,
            Type          = request.Type,
            Title         = request.Title,
            Message       = request.Message,
            ReferenceId   = request.ReferenceId,
            ReferenceType = request.ReferenceType
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return MapToDto(notification);
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id)
            ?? throw new Exception($"Notification {id} not found.");

        notification.IsRead  = true;
        notification.ReadAt  = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead    = true;
            n.ReadAt    = now;
            n.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id)
            ?? throw new Exception($"Notification {id} not found.");

        notification.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id            = n.Id,
        UserId        = n.UserId,
        Type          = n.Type.ToString(),
        Title         = n.Title,
        Message       = n.Message,
        ReferenceId   = n.ReferenceId,
        ReferenceType = n.ReferenceType,
        IsRead        = n.IsRead,
        ReadAt        = n.ReadAt,
        CreatedAt     = n.CreatedAt
    };
}
