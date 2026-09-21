namespace FleetManagement.Application.DTOs.Reports;

public class DriverPerformanceReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<DriverStatsDto> Drivers { get; set; } = new();
}

public class DriverStatsDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int TotalTrips { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public int IncidentsCount { get; set; }
    public int InspectionsFailed { get; set; }
}
