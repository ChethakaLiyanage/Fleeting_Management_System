using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Documents;

public class UploadDocumentRequest
{{
    public Guid? VehicleId {{ get; set; }}
    public Guid? DriverId {{ get; set; }}
    public Guid UploadedByUserId {{ get; set; }}
    public DocumentType DocumentType {{ get; set; }}
    public string? Description {{ get; set; }}
    public DateTime? ExpiryDate {{ get; set; }}
    // File content (bytes) and name passed in from controller
    public byte[] FileContent {{ get; set; }} = Array.Empty<byte>();
    public string OriginalFileName {{ get; set; }} = string.Empty;
    public string ContentType {{ get; set; }} = string.Empty;
}}
