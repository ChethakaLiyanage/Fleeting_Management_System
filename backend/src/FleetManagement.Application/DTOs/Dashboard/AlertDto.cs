namespace FleetManagement.Application.DTOs.Dashboard;

public class AlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;  // Low, Medium, High
    public string Message { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
    public DateTime? DueDate { get; set; }
}
