using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Drivers;

public class CreateDriverDto
{
    public string InitialPassword { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty;
    public DateTime LicenseIssueDate { get; set; }
    public DateTime LicenseExpiry { get; set; }
    public DateTime? JoinDate { get; set; }
    public string EmergencyContact { get; set; } = string.Empty;
}
