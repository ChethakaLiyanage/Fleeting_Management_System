using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public AuditAction Action { get; set; }

    /// <summary>Entity type affected (e.g. "Vehicle", "Driver", "Expense").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected entity.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>JSON snapshot of changes.</summary>
    public string? Changes { get; set; }

    /// <summary>Client IP or system identifier.</summary>
    public string? IpAddress { get; set; }
}
