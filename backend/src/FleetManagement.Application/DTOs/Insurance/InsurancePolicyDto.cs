namespace FleetManagement.Application.DTOs.Insurance;

public class InsurancePolicyDto
{{
    public Guid Id {{ get; set; }}
    public Guid VehicleId {{ get; set; }}
    public string? VehicleRegistration {{ get; set; }}
    public string PolicyType {{ get; set; }} = string.Empty;
    public string PolicyNumber {{ get; set; }} = string.Empty;
    public string Insurer {{ get; set; }} = string.Empty;
    public decimal PremiumAmount {{ get; set; }}
    public DateTime StartDate {{ get; set; }}
    public DateTime ExpiryDate {{ get; set; }}
    public string? CoverageDetails {{ get; set; }}
    public string? Notes {{ get; set; }}
    public bool IsExpired {{ get; set; }}
    public bool IsExpiringSoon {{ get; set; }}
    public int DaysUntilExpiry {{ get; set; }}
}}
