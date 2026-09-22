namespace FleetManagement.Application.DTOs.Reports;

public class CostReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal TotalInsuranceCost { get; set; }
    public decimal TotalOtherExpenses { get; set; }
    public decimal GrandTotal { get; set; }
    public List<VehicleCostDto> PerVehicle { get; set; } = new();
}

public class VehicleCostDto
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public decimal FuelCost { get; set; }
    public decimal MaintenanceCost { get; set; }
    public decimal OtherCost { get; set; }
    public decimal TotalCost { get; set; }
}
