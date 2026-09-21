namespace FleetManagement.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    // Fleet overview
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int VehiclesInUse { get; set; }
    public int VehiclesUnderMaintenance { get; set; }

    // Drivers
    public int TotalDrivers { get; set; }
    public int AvailableDrivers { get; set; }

    // Trips today
    public int TripsToday { get; set; }
    public int ActiveTrips { get; set; }

    // Alerts
    public int OverdueMaintenanceCount { get; set; }
    public int ExpiringInsuranceCount { get; set; }
    public int ExpiringDocumentCount { get; set; }
    public int PendingExpensesCount { get; set; }
    public int ExpiringLicensesCount { get; set; }

    // Costs this month
    public decimal FuelCostThisMonth { get; set; }
    public decimal MaintenanceCostThisMonth { get; set; }
    public decimal TotalCostThisMonth { get; set; }

    // Incidents
    public int OpenIncidentsCount { get; set; }
}
