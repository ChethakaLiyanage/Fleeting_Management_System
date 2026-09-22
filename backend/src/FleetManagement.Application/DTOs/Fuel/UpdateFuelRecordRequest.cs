using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Fuel;

public class UpdateFuelRecordRequest
{
    public Guid VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public DateTime FuelDate { get; set; }
    public decimal Litres { get; set; }
    public decimal CostPerLitre { get; set; }
    public decimal OdometerReading { get; set; }
    public FuelType FuelType { get; set; }
    public string? Station { get; set; }
    public string? Notes { get; set; }
}