using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Driver : BaseEntity
{
    public Guid UserId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty;
    public DateTime LicenseIssueDate { get; set; }
    public DateTime LicenseExpiry { get; set; }
    public DriverStatus Status { get; set; } = DriverStatus.Available;
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public string EmergencyContact { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public bool IsLicenseExpired => LicenseExpiry <= DateTime.UtcNow;
}
