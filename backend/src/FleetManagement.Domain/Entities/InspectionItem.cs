using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class InspectionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InspectionId { get; set; }
    public Inspection Inspection { get; set; } = null!;

    public string ItemName { get; set; } = string.Empty;
    public InspectionItemStatus Status { get; set; } = InspectionItemStatus.Pass;
    public string? Notes { get; set; }
}
