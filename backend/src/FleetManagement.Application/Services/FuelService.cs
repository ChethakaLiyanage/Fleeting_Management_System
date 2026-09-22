using FleetManagement.Application.DTOs.Fuel;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class FuelService : IFuelService
{
    private readonly IFleetDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<FuelService> _logger;

    public FuelService(
        IFleetDbContext context,
        ICurrentUser currentUser,
        IDashboardCache dashboardCache,
        ILogger<FuelService> logger)
    {
        _context        = context;
        _currentUser    = currentUser;
        _dashboardCache = dashboardCache;
        _logger         = logger;
    }

    public async Task<IEnumerable<FuelRecordDto>> GetAllAsync()
    {
        var query = _context.FuelRecords
            .Include(f => f.Vehicle)
            .Include(f => f.Driver)
            .AsNoTracking()
            .Where(f => !f.IsDeleted);

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId);

            if (driver == null)
            {
                return Enumerable.Empty<FuelRecordDto>();
            }

            query = query.Where(f => f.DriverId == driver.Id);
        }

        var records = await query
            .OrderByDescending(f => f.FuelDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<FuelRecordDto>> GetByVehicleAsync(Guid vehicleId)
    {
        var records = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Include(f => f.Driver)
            .AsNoTracking()
            .Where(f => f.VehicleId == vehicleId && !f.IsDeleted)
            .OrderByDescending(f => f.FuelDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<FuelRecordDto> GetByIdAsync(Guid id)
    {
        var record = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Include(f => f.Driver)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted)
            ?? throw new NotFoundException(nameof(FuelRecord), id);

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId);

            if (record.DriverId != driver?.Id)
            {
                throw new ForbiddenException("You can only access your own fuel records.");
            }
        }

        return MapToDto(record);
    }

    public async Task<FuelRecordDto> CreateAsync(CreateFuelRecordRequest request)
    {
        var driverId = request.DriverId;

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId);

            if (driver != null)
            {
                driverId = driver.Id;
            }
        }

        var record = new FuelRecord
        {
            VehicleId       = request.VehicleId,
            DriverId        = driverId,
            FuelDate        = request.FuelDate,
            Litres          = request.Litres,
            CostPerLitre    = request.CostPerLitre,
            OdometerReading = request.OdometerReading,
            FuelType        = request.FuelType,
            Station         = request.Station,
            Notes           = request.Notes,
            CreatedAt       = DateTime.UtcNow
        };

        _context.FuelRecords.Add(record);
        await _context.SaveChangesAsync();

        // Update vehicle mileage if odometer reading is higher
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId);
        if (vehicle != null && request.OdometerReading > vehicle.Mileage)
        {
            vehicle.Mileage   = request.OdometerReading;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Fuel record created: {Id} for vehicle {VehicleId}", record.Id, request.VehicleId);
        _dashboardCache.Invalidate();

        return await GetByIdAsync(record.Id);
    }

    public async Task<FuelRecordDto> UpdateAsync(Guid id, UpdateFuelRecordRequest request)
    {
        var record = await _context.FuelRecords.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted)
            ?? throw new NotFoundException(nameof(FuelRecord), id);
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId && !v.IsDeleted)
            ?? throw new NotFoundException($"Vehicle with ID '{request.VehicleId}' was not found.");
        if (request.DriverId.HasValue && !await _context.Drivers.AnyAsync(d => d.Id == request.DriverId.Value && !d.IsDeleted))
            throw new NotFoundException($"Driver with ID '{request.DriverId}' was not found.");
        if (request.Litres < 0 || request.CostPerLitre < 0 || request.OdometerReading < 0)
            throw new InvalidOperationException("Fuel quantities, prices, and odometer readings cannot be negative.");

        record.VehicleId = request.VehicleId;
        record.DriverId = request.DriverId;
        record.FuelDate = request.FuelDate;
        record.Litres = request.Litres;
        record.CostPerLitre = request.CostPerLitre;
        record.OdometerReading = request.OdometerReading;
        record.FuelType = request.FuelType;
        record.Station = request.Station?.Trim();
        record.Notes = request.Notes?.Trim();
        record.UpdatedAt = DateTime.UtcNow;
        if (request.OdometerReading > vehicle.Mileage)
        {
            vehicle.Mileage = request.OdometerReading;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        _dashboardCache.Invalidate();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await _context.FuelRecords.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted)
            ?? throw new NotFoundException(nameof(FuelRecord), id);

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogWarning("Fuel record deleted: {Id}", id);
        _dashboardCache.Invalidate();
    }

    public async Task<FuelEfficiencyDto> GetEfficiencyAsync(Guid vehicleId)
    {
        var records = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .AsNoTracking()
            .Where(f => f.VehicleId == vehicleId && !f.IsDeleted)
            .OrderBy(f => f.OdometerReading)
            .ToListAsync();

        var totalLitres    = records.Sum(r => r.Litres);
        var totalCost      = records.Sum(r => r.Litres * r.CostPerLitre);
        decimal kmPerLitre = 0;

        if (records.Count >= 2 && totalLitres > 0)
        {
            var distanceCovered = records.Last().OdometerReading - records.First().OdometerReading;
            kmPerLitre = distanceCovered > 0 ? Math.Round(distanceCovered / totalLitres, 2) : 0;
        }

        return new FuelEfficiencyDto
        {
            VehicleId            = vehicleId,
            VehicleRegistration  = records.FirstOrDefault()?.Vehicle?.RegistrationNumber,
            AverageKmPerLitre    = kmPerLitre,
            TotalFuelCost        = totalCost,
            TotalLitres          = totalLitres
        };
    }

    private static FuelRecordDto MapToDto(FuelRecord f) => new()
    {
        Id                  = f.Id,
        VehicleId           = f.VehicleId,
        VehicleRegistration = f.Vehicle?.RegistrationNumber ?? string.Empty,
        DriverId            = f.DriverId,
        DriverName          = f.Driver != null ? $"{f.Driver.FirstName} {f.Driver.LastName}" : null,
        FuelDate            = f.FuelDate,
        Litres              = f.Litres,
        CostPerLitre        = f.CostPerLitre,
        TotalCost           = f.TotalCost,
        OdometerReading     = f.OdometerReading,
        FuelType            = f.FuelType.ToString(),
        Station             = f.Station,
        Notes               = f.Notes
    };
}
