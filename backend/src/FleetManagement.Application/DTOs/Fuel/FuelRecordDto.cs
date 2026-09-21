namespace FleetManagement.Application.DTOs.Fuel;

public class FuelRecordDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistration { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public DateTime FuelDate { get; set; }
    public decimal Litres { get; set; }
    public decimal CostPerLitre { get; set; }
    public decimal TotalCost { get; set; }
    public decimal OdometerReading { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string? Station { get; set; }
    public string? Notes { get; set; }
}
