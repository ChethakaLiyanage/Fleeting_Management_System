using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Insurance;

public class CreateInsurancePolicyRequest
{{
    public Guid VehicleId {{ get; set; }}
    public InsurancePolicyType PolicyType {{ get; set; }}
    public string PolicyNumber {{ get; set; }} = string.Empty;
    public string Insurer {{ get; set; }} = string.Empty;
    public decimal PremiumAmount {{ get; set; }}
    public DateTime StartDate {{ get; set; }}
    public DateTime ExpiryDate {{ get; set; }}
    public string? CoverageDetails {{ get; set; }}
    public string? Notes {{ get; set; }}
}}
