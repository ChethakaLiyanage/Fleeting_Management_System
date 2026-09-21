using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Document : BaseEntity
{
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }

    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;   // UUID-prefixed on disk
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    /// <summary>Relative path inside the uploads folder.</summary>
    public string FilePath { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }

    // Business rules
    public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;
    public bool IsExpiringSoon =>
        ExpiryDate.HasValue && !IsExpired && (ExpiryDate.Value - DateTime.UtcNow).TotalDays <= 30;
}
