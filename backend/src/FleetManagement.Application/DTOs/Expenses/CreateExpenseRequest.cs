using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Expenses;

public class CreateExpenseRequest
{{
    public Guid? VehicleId {{ get; set; }}
    public Guid? DriverId {{ get; set; }}
    public Guid SubmittedByUserId {{ get; set; }}
    public ExpenseCategory Category {{ get; set; }}
    public string Description {{ get; set; }} = string.Empty;
    public decimal Amount {{ get; set; }}
    public DateTime ExpenseDate {{ get; set; }} = DateTime.UtcNow;
    public string? ReceiptReference {{ get; set; }}
    public string? Notes {{ get; set; }}
}}
