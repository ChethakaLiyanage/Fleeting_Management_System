using FleetManagement.Application.DTOs.Maintenance;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IFleetDbContext _context;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(
        IFleetDbContext context,
        IDashboardCache dashboardCache,
        ILogger<MaintenanceService> logger)
    {
        _context        = context;
        _dashboardCache = dashboardCache;
        _logger         = logger;
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetAllAsync()
    {
        return await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .Select(m => new MaintenanceRecordDto
            {
                Id                  = m.Id,
                VehicleId           = m.VehicleId,
                VehicleRegistration = m.Vehicle != null ? m.Vehicle.RegistrationNumber : string.Empty,
                Type                = m.Type,
                Description         = m.Description,
                ServiceProvider     = m.ServiceProvider,
                ScheduledDate       = m.ScheduledDate,
                CompletedDate       = m.CompletedDate,
                OdometerReading     = m.OdometerReading,
                Cost                = m.Cost,
                NextServiceOdometer = m.NextServiceOdometer,
                NextServiceDate     = m.NextServiceDate,
                Notes               = m.Notes,
                Status              = m.Status,
                IsOverdue           = m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < DateTime.UtcNow,
                IsCompleted         = m.Status == MaintenanceStatus.Completed,
                CreatedAt           = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetByVehicleAsync(Guid vehicleId)
    {
        return await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(m => m.VehicleId == vehicleId && !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .Select(m => new MaintenanceRecordDto
            {
                Id                  = m.Id,
                VehicleId           = m.VehicleId,
                VehicleRegistration = m.Vehicle != null ? m.Vehicle.RegistrationNumber : string.Empty,
                Type                = m.Type,
                Description         = m.Description,
                ServiceProvider     = m.ServiceProvider,
                ScheduledDate       = m.ScheduledDate,
                CompletedDate       = m.CompletedDate,
                OdometerReading     = m.OdometerReading,
                Cost                = m.Cost,
                NextServiceOdometer = m.NextServiceOdometer,
                NextServiceDate     = m.NextServiceDate,
                Notes               = m.Notes,
                Status              = m.Status,
                IsOverdue           = m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < DateTime.UtcNow,
                IsCompleted         = m.Status == MaintenanceStatus.Completed,
                CreatedAt           = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(m => m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now && !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .Select(m => new MaintenanceRecordDto
            {
                Id                  = m.Id,
                VehicleId           = m.VehicleId,
                VehicleRegistration = m.Vehicle != null ? m.Vehicle.RegistrationNumber : string.Empty,
                Type                = m.Type,
                Description         = m.Description,
                ServiceProvider     = m.ServiceProvider,
                ScheduledDate       = m.ScheduledDate,
                CompletedDate       = m.CompletedDate,
                OdometerReading     = m.OdometerReading,
                Cost                = m.Cost,
                NextServiceOdometer = m.NextServiceOdometer,
                NextServiceDate     = m.NextServiceDate,
                Notes               = m.Notes,
                Status              = m.Status,
                IsOverdue           = true,
                IsCompleted         = false,
                CreatedAt           = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<MaintenanceRecordDto> GetByIdAsync(Guid id)
    {
        var record = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new NotFoundException(nameof(MaintenanceRecord), id);

        return MapToDto(record);
    }

    public async Task<MaintenanceRecordDto> CreateAsync(CreateMaintenanceRequest request)
    {
        var record = new MaintenanceRecord
        {
            VehicleId           = request.VehicleId,
            Type                = request.Type,
            Description         = request.Description,
            ServiceProvider     = request.ServiceProvider,
            ScheduledDate       = request.ScheduledDate,
            OdometerReading     = request.OdometerReading,
            Cost                = request.Cost,
            NextServiceOdometer = request.NextServiceOdometer,
            NextServiceDate     = request.NextServiceDate,
            Notes               = request.Notes,
            Status              = MaintenanceStatus.Scheduled,
            CreatedAt           = DateTime.UtcNow
        };

        _context.MaintenanceRecords.Add(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Maintenance record created: {Id} for vehicle {VehicleId}", record.Id, request.VehicleId);
        _dashboardCache.Invalidate();

        return await GetByIdAsync(record.Id);
    }

    public async Task<MaintenanceRecordDto> UpdateAsync(Guid id, UpdateMaintenanceRequest request)
    {
        var record = await _context.MaintenanceRecords.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new NotFoundException(nameof(MaintenanceRecord), id);
        if (!await _context.Vehicles.AnyAsync(v => v.Id == request.VehicleId && !v.IsDeleted))
            throw new NotFoundException($"Vehicle with ID '{request.VehicleId}' was not found.");
        if (request.Cost < 0 || request.OdometerReading < 0 || request.NextServiceOdometer < 0)
            throw new InvalidOperationException("Maintenance costs and odometer readings cannot be negative.");

        record.VehicleId = request.VehicleId;
        record.Type = request.Type;
        record.Description = request.Description.Trim();
        record.ServiceProvider = request.ServiceProvider?.Trim();
        record.ScheduledDate = request.ScheduledDate;
        record.CompletedDate = request.CompletedDate;
        record.OdometerReading = request.OdometerReading;
        record.Cost = request.Cost;
        record.NextServiceOdometer = request.NextServiceOdometer;
        record.NextServiceDate = request.NextServiceDate;
        record.Status = request.Status;
        record.Notes = request.Notes?.Trim();
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _dashboardCache.Invalidate();
        return await GetByIdAsync(id);
    }

    public async Task<MaintenanceRecordDto> UpdateStatusAsync(Guid id, UpdateMaintenanceStatusRequest request)
    {
        var record = await _context.MaintenanceRecords.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new NotFoundException(nameof(MaintenanceRecord), id);

        record.Status    = request.Status;
        record.UpdatedAt = DateTime.UtcNow;

        if (request.Status == MaintenanceStatus.Completed)
        {
            record.CompletedDate = request.CompletedDate ?? DateTime.UtcNow;
            if (request.ActualCost.HasValue)
                record.Cost = request.ActualCost.Value;
        }

        if (request.Notes != null)
            record.Notes = request.Notes;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Maintenance status updated: {Id} -> {Status}", id, request.Status);
        _dashboardCache.Invalidate();

        return await GetByIdAsync(record.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await _context.MaintenanceRecords.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new NotFoundException(nameof(MaintenanceRecord), id);

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogWarning("Maintenance record deleted: {Id}", id);
        _dashboardCache.Invalidate();
    }

    private static MaintenanceRecordDto MapToDto(MaintenanceRecord m) => new()
    {
        Id                  = m.Id,
        VehicleId           = m.VehicleId,
        VehicleRegistration = m.Vehicle?.RegistrationNumber ?? string.Empty,
        Type                = m.Type,
        Description         = m.Description,
        ServiceProvider     = m.ServiceProvider,
        ScheduledDate       = m.ScheduledDate,
        CompletedDate       = m.CompletedDate,
        OdometerReading     = m.OdometerReading,
        Cost                = m.Cost,
        NextServiceOdometer = m.NextServiceOdometer,
        NextServiceDate     = m.NextServiceDate,
        Notes               = m.Notes,
        Status              = m.Status,
        IsOverdue           = m.IsOverdue,
        IsCompleted         = m.IsCompleted,
        CreatedAt           = m.CreatedAt
    };
}
