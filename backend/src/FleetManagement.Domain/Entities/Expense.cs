using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class Expense : BaseEntity
{
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    // Who submitted the expense
    public Guid? SubmittedByUserId { get; set; }
    public User? SubmittedByUser { get; set; }

    // Who approved/rejected
    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }

    public ExpenseCategory Category { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public string? ReceiptReference { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }

    // Business rules
    public bool CanBeApproved => Status == ExpenseStatus.Pending;
    public bool CanBeRejected => Status == ExpenseStatus.Pending;
}
