namespace FleetManagement.Application.DTOs.Expenses;

public class RejectExpenseRequest
{{
    public Guid RejectedByUserId {{ get; set; }}
    public string Reason {{ get; set; }} = string.Empty;
}}
