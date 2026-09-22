using FleetManagement.Application.DTOs.Reports;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly IFleetDbContext _context;

    public ReportService(IFleetDbContext context) => _context = context;

    public async Task<FleetUtilizationReportDto> GetFleetUtilizationAsync(DateTime from, DateTime to)
    {
        var trips = await _context.Trips
            .Include(t => t.Vehicle)
            .Where(t => !t.IsDeleted && t.StartTime >= from && t.StartTime <= to)
            .ToListAsync();

        var vehicleGroups = trips.GroupBy(t => t.VehicleId);
        var totalPeriodHours = (to - from).TotalHours;

        var vehicleStats = vehicleGroups.Select(g => new VehicleUtilizationDto
        {
            VehicleId           = g.Key,
            RegistrationNumber  = g.First().Vehicle?.RegistrationNumber ?? "-",
            TotalTrips          = g.Count(),
            TotalDistanceKm     = g.Sum(t => t.EndMileage.HasValue ? t.EndMileage.Value - t.StartMileage : 0),
            TotalTripHours      = (decimal)g.Where(t => t.EndTime.HasValue)
                                            .Sum(t => (t.EndTime!.Value - t.StartTime).TotalHours),
            UtilizationPercent  = totalPeriodHours > 0
                ? Math.Round((decimal)g.Where(t => t.EndTime.HasValue)
                                       .Sum(t => (t.EndTime!.Value - t.StartTime).TotalHours)
                             / (decimal)totalPeriodHours * 100, 2)
                : 0
        }).ToList();

        var overallUtil = vehicleStats.Count > 0
            ? Math.Round(vehicleStats.Average(v => v.UtilizationPercent), 2) : 0;

        return new FleetUtilizationReportDto
        {
            From                      = from,
            To                        = to,
            Vehicles                  = vehicleStats,
            OverallUtilizationPercent = overallUtil
        };
    }

    public async Task<CostReportDto> GetCostReportAsync(DateTime from, DateTime to)
    {
        var fuelRecords = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Where(f => !f.IsDeleted && f.FuelDate >= from && f.FuelDate <= to)
            .ToListAsync();

        var mainRecords = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => !m.IsDeleted && m.CompletedDate >= from && m.CompletedDate <= to && m.Cost.HasValue)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= from && e.ExpenseDate <= to
                && e.Status == ExpenseStatus.Approved)
            .ToListAsync();

        var allVehicleIds = fuelRecords.Select(f => f.VehicleId)
            .Union(mainRecords.Select(m => m.VehicleId))
            .Distinct();

        var perVehicle = allVehicleIds.Select(vid => new VehicleCostDto
        {
            VehicleId           = vid,
            RegistrationNumber  = fuelRecords.FirstOrDefault(f => f.VehicleId == vid)?.Vehicle?.RegistrationNumber
                                  ?? mainRecords.FirstOrDefault(m => m.VehicleId == vid)?.Vehicle?.RegistrationNumber
                                  ?? "-",
            FuelCost            = fuelRecords.Where(f => f.VehicleId == vid).Sum(f => f.TotalCost),
            MaintenanceCost     = mainRecords.Where(m => m.VehicleId == vid).Sum(m => m.Cost!.Value),
            OtherCost           = expenses.Where(e => e.VehicleId == vid).Sum(e => e.Amount),
            TotalCost           = fuelRecords.Where(f => f.VehicleId == vid).Sum(f => f.TotalCost)
                                  + mainRecords.Where(m => m.VehicleId == vid).Sum(m => m.Cost!.Value)
                                  + expenses.Where(e => e.VehicleId == vid).Sum(e => e.Amount)
        }).ToList();

        return new CostReportDto
        {
            From                    = from,
            To                      = to,
            TotalFuelCost           = fuelRecords.Sum(f => f.TotalCost),
            TotalMaintenanceCost    = mainRecords.Sum(m => m.Cost!.Value),
            TotalInsuranceCost      = 0,   // insurance premiums tracked separately
            TotalOtherExpenses      = expenses.Sum(e => e.Amount),
            GrandTotal              = fuelRecords.Sum(f => f.TotalCost)
                                      + mainRecords.Sum(m => m.Cost!.Value)
                                      + expenses.Sum(e => e.Amount),
            PerVehicle              = perVehicle
        };
    }

    public async Task<DriverPerformanceReportDto> GetDriverPerformanceAsync(DateTime from, DateTime to)
    {
        var trips = await _context.Trips
            .Include(t => t.Driver)
            .Where(t => !t.IsDeleted && t.StartTime >= from && t.StartTime <= to && t.DriverId.HasValue)
            .ToListAsync();

        var incidents = await _context.Incidents
            .Where(i => !i.IsDeleted && i.ReportedAt >= from && i.ReportedAt <= to && i.DriverId.HasValue)
            .ToListAsync();

        var inspections = await _context.Inspections
            .Where(i => !i.IsDeleted && i.InspectedAt >= from && i.InspectedAt <= to
                && i.Result == InspectionResult.Failed)
            .ToListAsync();

        var driverIds = trips.Select(t => t.DriverId!.Value).Distinct();

        var driverStats = driverIds.Select(did => new DriverStatsDto
        {
            DriverId         = did,
            DriverName       = trips.First(t => t.DriverId == did).Driver != null
                               ? $"{trips.First(t => t.DriverId == did).Driver!.FirstName} {trips.First(t => t.DriverId == did).Driver!.LastName}"
                               : "-",
            TotalTrips       = trips.Count(t => t.DriverId == did),
            ActiveTrips      = trips.Count(t => t.DriverId == did && t.Status == TripStatus.InProgress),
            TotalDistanceKm  = trips.Where(t => t.DriverId == did)
                                    .Sum(t => t.EndMileage.HasValue ? t.EndMileage.Value - t.StartMileage : 0),
            IncidentsCount   = incidents.Count(i => i.DriverId == did),
            InspectionsFailed = inspections.Count(i => i.DriverId == did)
        }).ToList();

        return new DriverPerformanceReportDto
        {
            From    = from,
            To      = to,
            Drivers = driverStats
        };
    }
}
