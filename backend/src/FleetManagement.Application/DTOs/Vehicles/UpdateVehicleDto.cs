using FleetManagement.Domain.Enums;

namespace FleetManagement.Application.DTOs.Vehicles;

public class UpdateVehicleDto
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? VIN { get; set; }
    public string EngineNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public VehicleType VehicleType { get; set; }
    public FuelType FuelType { get; set; }
    public TransmissionType Transmission { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal Mileage { get; set; }
    public VehicleStatus Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? RegistrationExpiry { get; set; }
}
