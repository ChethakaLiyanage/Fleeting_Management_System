using FleetManagement.Application.DTOs.Notifications;

namespace FleetManagement.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetByUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid id);
}
