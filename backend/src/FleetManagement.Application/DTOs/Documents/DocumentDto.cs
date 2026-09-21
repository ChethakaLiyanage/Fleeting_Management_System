namespace FleetManagement.Application.DTOs.Documents;

public class DocumentDto
{{
    public Guid Id {{ get; set; }}
    public Guid? VehicleId {{ get; set; }}
    public string? VehicleRegistration {{ get; set; }}
    public Guid? DriverId {{ get; set; }}
    public string? DriverName {{ get; set; }}
    public string DocumentType {{ get; set; }} = string.Empty;
    public string FileName {{ get; set; }} = string.Empty;
    public string ContentType {{ get; set; }} = string.Empty;
    public long FileSizeBytes {{ get; set; }}
    public DateTime? ExpiryDate {{ get; set; }}
    public string? Description {{ get; set; }}
    public bool IsExpired {{ get; set; }}
    public bool IsExpiringSoon {{ get; set; }}
    public DateTime UploadedAt {{ get; set; }}
}}
