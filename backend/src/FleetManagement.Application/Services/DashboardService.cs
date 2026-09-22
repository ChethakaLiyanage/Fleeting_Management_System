using FleetManagement.Application.DTOs.Dashboard;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Enums;
using FleetManagement.Infrastructure.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FleetManagement.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IFleetDbContext _context;
    private readonly IMemoryCache _cache;

    public DashboardService(IFleetDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache   = cache;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        if (_cache.TryGetValue(DashboardCacheInvalidator.CacheKey, out DashboardSummaryDto? cached) && cached != null)
        {
            return cached;
        }

        var summary = await BuildSummaryAsync();
        _cache.Set(DashboardCacheInvalidator.CacheKey, summary, TimeSpan.FromMinutes(5));
        return summary;
    }

    private async Task<DashboardSummaryDto> BuildSummaryAsync()
    {
        var now          = DateTime.UtcNow;
        var todayUtc     = now.Date;
        var tomorrowUtc  = todayUtc.AddDays(1);
        var monthStart   = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var expiryWindow = now.AddDays(30);

        // Run CountAsync in database rather than loading entire tables into memory
        var totalVehicles            = await _context.Vehicles.CountAsync(v => !v.IsDeleted);
        var availableVehicles        = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.Available);
        var vehiclesInUse           = await _context.Vehicles.CountAsync(v => !v.IsDeleted && (v.Status == VehicleStatus.InUse || v.Status == VehicleStatus.OnTrip));
        var vehiclesUnderMaintenance = await _context.Vehicles.CountAsync(v => !v.IsDeleted && (v.Status == VehicleStatus.UnderMaintenance || v.Status == VehicleStatus.Maintenance));

        var totalDrivers             = await _context.Drivers.CountAsync(d => !d.IsDeleted);
        var availableDrivers         = await _context.Drivers.CountAsync(d => !d.IsDeleted && d.Status == DriverStatus.Available);

        var tripsToday               = await _context.Trips.CountAsync(t => !t.IsDeleted && t.StartTime >= todayUtc && t.StartTime < tomorrowUtc);
        var activeTrips              = await _context.Trips.CountAsync(t => !t.IsDeleted && t.Status == TripStatus.InProgress);

        var overdueMain = await _context.MaintenanceRecords
            .CountAsync(m => !m.IsDeleted && m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now);

        var expiringIns = await _context.InsurancePolicies
            .CountAsync(p => !p.IsDeleted && p.ExpiryDate >= now && p.ExpiryDate <= expiryWindow);

        var expiringDocs = await _context.Documents
            .CountAsync(d => !d.IsDeleted && d.ExpiryDate.HasValue && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= expiryWindow);

        var pendingExp = await _context.Expenses
            .CountAsync(e => !e.IsDeleted && e.Status == ExpenseStatus.Pending);

        var openIncidents = await _context.Incidents
            .CountAsync(i => !i.IsDeleted && i.Status != IncidentStatus.Closed);

        var expiringLic = await _context.Drivers
            .CountAsync(d => !d.IsDeleted && d.LicenseExpiry >= now && d.LicenseExpiry <= expiryWindow);

        var fuelCost = await _context.FuelRecords
            .Where(f => !f.IsDeleted && f.FuelDate >= monthStart)
            .SumAsync(f => f.Litres * f.CostPerLitre);

        var mainCost = await _context.MaintenanceRecords
            .Where(m => !m.IsDeleted && m.CompletedDate >= monthStart && m.Cost.HasValue)
            .SumAsync(m => m.Cost!.Value);

        return new DashboardSummaryDto
        {
            TotalVehicles            = totalVehicles,
            AvailableVehicles        = availableVehicles,
            VehiclesInUse           = vehiclesInUse,
            VehiclesUnderMaintenance = vehiclesUnderMaintenance,
            TotalDrivers             = totalDrivers,
            AvailableDrivers         = availableDrivers,
            TripsToday               = tripsToday,
            ActiveTrips              = activeTrips,
            OverdueMaintenanceCount  = overdueMain,
            ExpiringInsuranceCount   = expiringIns,
            ExpiringDocumentCount    = expiringDocs,
            PendingExpensesCount     = pendingExp,
            ExpiringLicensesCount    = expiringLic,
            FuelCostThisMonth        = fuelCost,
            MaintenanceCostThisMonth = mainCost,
            TotalCostThisMonth       = fuelCost + mainCost,
            OpenIncidentsCount       = openIncidents
        };
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var now    = DateTime.UtcNow;
        var window = now.AddDays(30);
        var alerts = new List<AlertDto>();

        var overdue = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => !m.IsDeleted && m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now)
            .AsNoTracking()
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

        var expiringPolicies = await _context.InsurancePolicies
            .Include(p => p.Vehicle)
            .Where(p => !p.IsDeleted && p.ExpiryDate >= now && p.ExpiryDate <= window)
            .AsNoTracking()
            .ToListAsync();

        alerts.AddRange(expiringPolicies.Select(p => new AlertDto
        {
            Type       = "InsuranceExpiring",
            Severity   = "Medium",
            Message    = $"Insurance policy {p.PolicyNumber} for {p.Vehicle?.RegistrationNumber} expires in {(p.ExpiryDate - now).Days} days",
            EntityId   = p.Id.ToString(),
            EntityName = p.Vehicle?.RegistrationNumber,
            DueDate    = p.ExpiryDate
        }));

        var expiringDocs = await _context.Documents
            .Where(d => !d.IsDeleted && d.ExpiryDate.HasValue && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= window)
            .AsNoTracking()
            .ToListAsync();

        alerts.AddRange(expiringDocs.Select(d => new AlertDto
        {
            Type       = "DocumentExpiring",
            Severity   = "Low",
            Message    = $"Document '{d.Title}' expires in {(d.ExpiryDate!.Value - now).Days} days",
            EntityId   = d.Id.ToString(),
            EntityName = d.Title,
            DueDate    = d.ExpiryDate
        }));

        var expiringDrivers = await _context.Drivers
            .Where(d => !d.IsDeleted && d.LicenseExpiry >= now && d.LicenseExpiry <= window)
            .AsNoTracking()
            .ToListAsync();

        alerts.AddRange(expiringDrivers.Select(d => new AlertDto
        {
            Type       = "LicenseExpiring",
            Severity   = "High",
            Message    = $"Driving license for {d.FullName} expires in {(d.LicenseExpiry - now).Days} days",
            EntityId   = d.Id.ToString(),
            EntityName = d.FullName,
            DueDate    = d.LicenseExpiry
        }));

        return alerts.OrderBy(a => a.DueDate);
    }
}
