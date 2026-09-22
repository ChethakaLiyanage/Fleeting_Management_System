using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional deep-link reference (e.g. vehicle id, insurance policy id).</summary>
    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
