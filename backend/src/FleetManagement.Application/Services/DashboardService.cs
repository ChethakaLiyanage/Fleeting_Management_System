using FleetManagement.Application.DTOs.Dashboard;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IFleetDbContext _context;

    public DashboardService(IFleetDbContext context) => _context = context;

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var now    = DateTime.UtcNow;
        var today  = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var expiryWindow = now.AddDays(30);

        // Vehicles
        var vehicles = await _context.Vehicles.Where(v => !v.IsDeleted).ToListAsync();
        // Drivers
        var drivers  = await _context.Drivers.Where(d => !d.IsDeleted).ToListAsync();
        // Trips today
        var trips    = await _context.Trips.Where(t => !t.IsDeleted).ToListAsync();
        // Maintenance overdue
        var overdueMain = await _context.MaintenanceRecords
            .Where(m => !m.IsDeleted && m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now)
            .CountAsync();
        // Insurance expiring
        var expiringIns = await _context.InsurancePolicies
            .Where(p => !p.IsDeleted && p.ExpiryDate >= now && p.ExpiryDate <= expiryWindow)
            .CountAsync();
        // Documents expiring
        var expiringDocs = await _context.Documents
            .Where(d => !d.IsDeleted && d.ExpiryDate.HasValue
                && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= expiryWindow)
            .CountAsync();
        // Pending expenses
        var pendingExp = await _context.Expenses
            .Where(e => !e.IsDeleted && e.Status == ExpenseStatus.Pending)
            .CountAsync();
        // Open incidents
        var openIncidents = await _context.Incidents
            .Where(i => !i.IsDeleted && i.Status != IncidentStatus.Closed)
            .CountAsync();
        // Expiring driver licenses
        var expiringLic = drivers.Count(d => d.LicenseExpiry >= now && d.LicenseExpiry <= expiryWindow);

        // Fuel cost this month
        var fuelCost = await _context.FuelRecords
            .Where(f => !f.IsDeleted && f.FuelDate >= monthStart)
            .SumAsync(f => f.Litres * f.CostPerLitre);
        // Maintenance cost this month
        var mainCost = await _context.MaintenanceRecords
            .Where(m => !m.IsDeleted && m.CompletedDate >= monthStart && m.Cost.HasValue)
            .SumAsync(m => m.Cost!.Value);

        return new DashboardSummaryDto
        {
            TotalVehicles              = vehicles.Count,
            AvailableVehicles          = vehicles.Count(v => v.Status == VehicleStatus.Available),
            VehiclesInUse             = vehicles.Count(v => v.Status == VehicleStatus.InUse),
            VehiclesUnderMaintenance   = vehicles.Count(v => v.Status == VehicleStatus.UnderMaintenance),
            TotalDrivers               = drivers.Count,
            AvailableDrivers           = drivers.Count(d => d.Status == DriverStatus.Available),
            TripsToday                 = trips.Count(t => t.StartTime.Date == today),
            ActiveTrips                = trips.Count(t => t.Status == TripStatus.InProgress),
            OverdueMaintenanceCount    = overdueMain,
            ExpiringInsuranceCount     = expiringIns,
            ExpiringDocumentCount      = expiringDocs,
            PendingExpensesCount       = pendingExp,
            ExpiringLicensesCount      = expiringLic,
            FuelCostThisMonth          = fuelCost,
            MaintenanceCostThisMonth   = mainCost,
            TotalCostThisMonth         = fuelCost + mainCost,
            OpenIncidentsCount         = openIncidents
        };
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var now     = DateTime.UtcNow;
        var window  = now.AddDays(30);
        var alerts  = new List<AlertDto>();

        // Overdue maintenance
        var overdue = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => !m.IsDeleted && m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now)
            .ToListAsync();

        alerts.AddRange(overdue.Select(m => new AlertDto
        {
            Type       = "MaintenanceOverdue",
            Severity   = "High",
            Message    = $"Maintenance overdue for {m.Vehicle?.RegistrationNumber}: {m.Description}",
            EntityId   = m.Id.ToString(),
            EntityName = m.Vehicle?.RegistrationNumber,
            DueDate    = m.ScheduledDate
        }));

        // Expiring insurance
        var insurance = await _context.InsurancePolicies
            .Include(p => p.Vehicle)
            .Where(p => !p.IsDeleted && p.ExpiryDate >= now && p.ExpiryDate <= window)
            .ToListAsync();

        alerts.AddRange(insurance.Select(p => new AlertDto
        {
            Type       = "InsuranceExpiry",
            Severity   = (p.ExpiryDate - now).TotalDays <= 7 ? "High" : "Medium",
            Message    = $"Insurance policy {p.PolicyNumber} for {p.Vehicle?.RegistrationNumber} expires on {p.ExpiryDate:dd MMM yyyy}",
            EntityId   = p.Id.ToString(),
            EntityName = p.Vehicle?.RegistrationNumber,
            DueDate    = p.ExpiryDate
        }));

        // Expiring documents
        var docs = await _context.Documents
            .Include(d => d.Vehicle)
            .Where(d => !d.IsDeleted && d.ExpiryDate.HasValue
                && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= window)
            .ToListAsync();

        alerts.AddRange(docs.Select(d => new AlertDto
        {
            Type       = "DocumentExpiry",
            Severity   = (d.ExpiryDate!.Value - now).TotalDays <= 7 ? "High" : "Medium",
            Message    = $"Document '{d.FileName}' expires on {d.ExpiryDate:dd MMM yyyy}",
            EntityId   = d.Id.ToString(),
            EntityName = d.FileName,
            DueDate    = d.ExpiryDate
        }));

        // Expiring driver licenses
        var drivers = await _context.Drivers
            .Where(d => !d.IsDeleted && d.LicenseExpiry >= now && d.LicenseExpiry <= window)
            .ToListAsync();

        alerts.AddRange(drivers.Select(d => new AlertDto
        {
            Type       = "LicenseExpiry",
            Severity   = (d.LicenseExpiry - now).TotalDays <= 7 ? "High" : "Medium",
            Message    = $"Driver license for {d.FirstName} {d.LastName} expires on {d.LicenseExpiry:dd MMM yyyy}",
            EntityId   = d.Id.ToString(),
            EntityName = $"{d.FirstName} {d.LastName}",
            DueDate    = d.LicenseExpiry
        }));

        return alerts.OrderByDescending(a => a.Severity).ThenBy(a => a.DueDate);
    }
}
