using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class InsurancePolicy : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public InsurancePolicyType PolicyType { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;

    public decimal PremiumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public string? CoverageDetails { get; set; }
    public string? Notes { get; set; }

    // Business rules
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    public bool IsExpiringSoon => !IsExpired && (ExpiryDate - DateTime.UtcNow).TotalDays <= 30;
    public int DaysUntilExpiry => (int)(ExpiryDate - DateTime.UtcNow).TotalDays;
}
