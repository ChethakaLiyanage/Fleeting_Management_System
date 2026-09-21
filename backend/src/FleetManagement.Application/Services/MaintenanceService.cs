using FleetManagement.Application.DTOs.Maintenance;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IFleetDbContext _context;

    public MaintenanceService(IFleetDbContext context) => _context = context;

    public async Task<IEnumerable<MaintenanceRecordDto>> GetAllAsync()
    {
        var records = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetByVehicleAsync(Guid vehicleId)
    {
        var records = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;
        var records = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<MaintenanceRecordDto> GetByIdAsync(Guid id)
    {
        var record = await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new Exception($"Maintenance record {id} not found.");

        return MapToDto(record);
    }

    public async Task<MaintenanceRecordDto> CreateAsync(CreateMaintenanceRequest request)
    {
        var record = new MaintenanceRecord
        {
            VehicleId            = request.VehicleId,
            Type                 = request.Type,
            Description          = request.Description,
            ServiceProvider      = request.ServiceProvider,
            ScheduledDate        = request.ScheduledDate,
            OdometerReading      = request.OdometerReading,
            Cost                 = request.Cost,
            NextServiceOdometer  = request.NextServiceOdometer,
            NextServiceDate      = request.NextServiceDate,
            Notes                = request.Notes,
            Status               = MaintenanceStatus.Scheduled
        };

        _context.MaintenanceRecords.Add(record);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(record.Id);
    }

    public async Task<MaintenanceRecordDto> UpdateStatusAsync(Guid id, UpdateMaintenanceStatusRequest request)
    {
        var record = await _context.MaintenanceRecords.FindAsync(id)
            ?? throw new Exception($"Maintenance record {id} not found.");

        record.Status        = request.Status;
        record.UpdatedAt     = DateTime.UtcNow;

        if (request.Status == MaintenanceStatus.Completed)
        {
            record.CompletedDate = request.CompletedDate ?? DateTime.UtcNow;
            if (request.ActualCost.HasValue)
                record.Cost = request.ActualCost.Value;
        }

        if (request.Notes != null)
            record.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await _context.MaintenanceRecords.FindAsync(id)
            ?? throw new Exception($"Maintenance record {id} not found.");

        record.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private static MaintenanceRecordDto MapToDto(MaintenanceRecord m) => new()
    {
        Id                  = m.Id,
        VehicleId           = m.VehicleId,
        VehicleRegistration = m.Vehicle?.RegistrationNumber,
        Type                = m.Type.ToString(),
        Status              = m.Status.ToString(),
        Description         = m.Description,
        ServiceProvider     = m.ServiceProvider,
        ScheduledDate       = m.ScheduledDate,
        CompletedDate       = m.CompletedDate,
        OdometerReading     = m.OdometerReading,
        Cost                = m.Cost,
        NextServiceOdometer = m.NextServiceOdometer,
        NextServiceDate     = m.NextServiceDate,
        Notes               = m.Notes,
        IsOverdue           = m.IsOverdue
    };
}
