namespace FleetManagement.Application.DTOs.Expenses;

public class ExpenseDto
{{
    public Guid Id {{ get; set; }}
    public Guid? VehicleId {{ get; set; }}
    public string? VehicleRegistration {{ get; set; }}
    public Guid? DriverId {{ get; set; }}
    public string? DriverName {{ get; set; }}
    public string Category {{ get; set; }} = string.Empty;
    public string Status {{ get; set; }} = string.Empty;
    public string Description {{ get; set; }} = string.Empty;
    public decimal Amount {{ get; set; }}
    public DateTime ExpenseDate {{ get; set; }}
    public string? ReceiptReference {{ get; set; }}
    public string? Notes {{ get; set; }}
    public DateTime? ApprovedAt {{ get; set; }}
    public string? RejectionReason {{ get; set; }}
}}
