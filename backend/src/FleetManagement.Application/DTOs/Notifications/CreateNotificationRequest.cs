using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Notifications;

public class CreateNotificationRequest
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}
