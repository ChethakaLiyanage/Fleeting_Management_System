namespace FleetManagement.Application.DTOs.Fuel;

public class FuelEfficiencyDto
{
    public Guid VehicleId { get; set; }
    public string? VehicleRegistration { get; set; }
    /// <summary>Average fuel efficiency in km per litre.</summary>
    public decimal AverageKmPerLitre { get; set; }
    /// <summary>Total fuel cost over the period.</summary>
    public decimal TotalFuelCost { get; set; }
    /// <summary>Total litres consumed over the period.</summary>
    public decimal TotalLitres { get; set; }
}
