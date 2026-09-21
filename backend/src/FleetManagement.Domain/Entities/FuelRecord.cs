using FleetManagement.Domain.Common;
using FleetManagement.Domain.Enums;

namespace FleetManagement.Domain.Entities;

public class FuelRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    /// <summary>Date and time fuel was filled.</summary>
    public DateTime FuelDate { get; set; } = DateTime.UtcNow;

    /// <summary>Litres added.</summary>
    public decimal Litres { get; set; }

    /// <summary>Cost per litre.</summary>
    public decimal CostPerLitre { get; set; }

    /// <summary>Total cost = Litres * CostPerLitre.</summary>
    public decimal TotalCost => Litres * CostPerLitre;

    /// <summary>Odometer reading at time of refuelling (km).</summary>
    public decimal OdometerReading { get; set; }

    public FuelType FuelType { get; set; }

    public string? Station { get; set; }
    public string? Notes { get; set; }

    // Business rule: fuel efficiency (km/L) can be calculated externally when
    // previous odometer reading is available.
}
