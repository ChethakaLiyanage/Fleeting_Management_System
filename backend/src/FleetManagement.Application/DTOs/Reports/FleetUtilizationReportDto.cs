namespace FleetManagement.Application.DTOs.Reports;

public class FleetUtilizationReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<VehicleUtilizationDto> Vehicles { get; set; } = new();
    public decimal OverallUtilizationPercent { get; set; }
}

public class VehicleUtilizationDto
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public int TotalTrips { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalTripHours { get; set; }
    public decimal UtilizationPercent { get; set; }
}
