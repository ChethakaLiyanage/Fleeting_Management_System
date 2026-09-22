using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Vehicles;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IFleetDbContext _context;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(
        IFleetDbContext context,
        IDashboardCache dashboardCache,
        ILogger<VehicleService> logger)
    {
        _context        = context;
        _dashboardCache = dashboardCache;
        _logger         = logger;
    }

    public async Task<PagedResult<VehicleDto>> GetVehiclesAsync(VehicleFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var search = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(v =>
                v.RegistrationNumber.ToLower().Contains(search) ||
                (v.VIN != null && v.VIN.ToLower().Contains(search)) ||
                v.Make.ToLower().Contains(search) ||
                v.Model.ToLower().Contains(search));
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(v => v.Status == filterParams.Status.Value);
        }

        if (filterParams.VehicleType.HasValue)
        {
            query = query.Where(v => v.VehicleType == filterParams.VehicleType.Value);
        }

        if (filterParams.FuelType.HasValue)
        {
            query = query.Where(v => v.FuelType == filterParams.FuelType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Make))
        {
            query = query.Where(v => v.Make.ToLower() == filterParams.Make.Trim().ToLower());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filterParams.SortBy?.ToLower() switch
        {
            "make" => filterParams.IsDescending ? query.OrderByDescending(v => v.Make) : query.OrderBy(v => v.Make),
            "mileage" => filterParams.IsDescending ? query.OrderByDescending(v => v.Mileage) : query.OrderBy(v => v.Mileage),
            "year" => filterParams.IsDescending ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
            "registrationexpiry" => filterParams.IsDescending ? query.OrderByDescending(v => v.RegistrationExpiry) : query.OrderBy(v => v.RegistrationExpiry),
            _ => filterParams.IsDescending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt)
        };

        // Project directly to DTO in SQL
        var items = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(v => new VehicleDto
            {
                Id                 = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                VIN                = v.VIN,
                EngineNumber       = v.EngineNumber,
                Make               = v.Make,
                Model              = v.Model,
                Year               = v.Year,
                VehicleType        = v.VehicleType,
                FuelType           = v.FuelType,
                Transmission       = v.Transmission,
                Color              = v.Color,
                Mileage            = v.Mileage,
                Status             = v.Status,
                PurchaseDate       = v.PurchaseDate,
                PurchasePrice      = v.PurchasePrice,
                RegistrationExpiry = v.RegistrationExpiry,
                CreatedAt          = v.CreatedAt,
                UpdatedAt          = v.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<VehicleDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id && !v.IsDeleted)
            .Select(v => new VehicleDto
            {
                Id                 = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                VIN                = v.VIN,
                EngineNumber       = v.EngineNumber,
                Make               = v.Make,
                Model              = v.Model,
                Year               = v.Year,
                VehicleType        = v.VehicleType,
                FuelType           = v.FuelType,
                Transmission       = v.Transmission,
                Color              = v.Color,
                Mileage            = v.Mileage,
                Status             = v.Status,
                PurchaseDate       = v.PurchaseDate,
                PurchasePrice      = v.PurchasePrice,
                RegistrationExpiry = v.RegistrationExpiry,
                CreatedAt          = v.CreatedAt,
                UpdatedAt          = v.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return vehicle;
    }

    public async Task<VehicleSummaryDto?> GetVehicleSummaryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

        if (vehicle == null) return null;

        var isExpiringSoon = vehicle.RegistrationExpiry.HasValue &&
            vehicle.RegistrationExpiry.Value <= DateTime.UtcNow.AddDays(30) &&
            vehicle.RegistrationExpiry.Value >= DateTime.UtcNow;

        return new VehicleSummaryDto
        {
            Id                         = vehicle.Id,
            RegistrationNumber         = vehicle.RegistrationNumber,
            Make                       = vehicle.Make,
            Model                      = vehicle.Model,
            Status                     = vehicle.Status,
            CurrentMileage             = vehicle.Mileage,
            RegistrationExpiry         = vehicle.RegistrationExpiry,
            IsRegistrationExpiringSoon = isExpiringSoon,
            TotalTrips                 = 0,
            TotalInspections           = 0,
            TotalIncidents             = 0
        };
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Vehicles
            .AnyAsync(v => v.RegistrationNumber.ToLower() == dto.RegistrationNumber.ToLower() && !v.IsDeleted, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Vehicle with registration number '{dto.RegistrationNumber}' already exists.");
        }

        var vehicle = new Vehicle
        {
            RegistrationNumber = dto.RegistrationNumber.Trim().ToUpper(),
            VIN                = dto.VIN?.Trim().ToUpper(),
            EngineNumber       = dto.EngineNumber.Trim(),
            Make               = dto.Make.Trim(),
            Model              = dto.Model.Trim(),
            Year               = dto.Year,
            VehicleType        = dto.VehicleType,
            FuelType           = dto.FuelType,
            Transmission       = dto.Transmission,
            Color              = dto.Color.Trim(),
            Mileage            = dto.Mileage,
            Status             = VehicleStatus.Available,
            PurchaseDate       = dto.PurchaseDate,
            PurchasePrice      = dto.PurchasePrice,
            RegistrationExpiry = dto.RegistrationExpiry,
            CreatedAt          = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vehicle created: {Id} {Reg}", vehicle.Id, vehicle.RegistrationNumber);
        _dashboardCache.Invalidate();

        return MapToDto(vehicle);
    }

    public async Task<VehicleDto?> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

        if (vehicle == null) return null;

        var duplicateReg = await _context.Vehicles
            .AnyAsync(v => v.Id != id && v.RegistrationNumber.ToLower() == dto.RegistrationNumber.ToLower() && !v.IsDeleted, cancellationToken);

        if (duplicateReg)
        {
            throw new ConflictException($"Another vehicle with registration number '{dto.RegistrationNumber}' already exists.");
        }

        vehicle.RegistrationNumber = dto.RegistrationNumber.Trim().ToUpper();
        vehicle.VIN                = dto.VIN?.Trim().ToUpper();
        vehicle.EngineNumber       = dto.EngineNumber.Trim();
        vehicle.Make               = dto.Make.Trim();
        vehicle.Model              = dto.Model.Trim();
        vehicle.Year               = dto.Year;
        vehicle.VehicleType        = dto.VehicleType;
        vehicle.FuelType           = dto.FuelType;
        vehicle.Transmission       = dto.Transmission;
        vehicle.Color              = dto.Color.Trim();
        vehicle.Mileage            = dto.Mileage;
        vehicle.Status             = dto.Status;
        vehicle.PurchaseDate       = dto.PurchaseDate;
        vehicle.PurchasePrice      = dto.PurchasePrice;
        vehicle.RegistrationExpiry = dto.RegistrationExpiry;
        vehicle.UpdatedAt          = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vehicle updated: {Id} {Reg}", vehicle.Id, vehicle.RegistrationNumber);
        _dashboardCache.Invalidate();

        return MapToDto(vehicle);
    }

    public async Task<bool> ArchiveVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

        if (vehicle == null) return false;

        if (await _context.Trips.AnyAsync(t => t.VehicleId == id && !t.IsDeleted && t.Status != TripStatus.Completed && t.Status != TripStatus.Cancelled, cancellationToken))
            throw new InvalidOperationException("Cannot archive a vehicle with an active trip.");

        vehicle.Status    = VehicleStatus.Retired;
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Vehicle archived: {Id}", id);
        _dashboardCache.Invalidate();

        return true;
    }

    private static VehicleDto MapToDto(Vehicle vehicle) => new()
    {
        Id                 = vehicle.Id,
        RegistrationNumber = vehicle.RegistrationNumber,
        VIN                = vehicle.VIN,
        EngineNumber       = vehicle.EngineNumber,
        Make               = vehicle.Make,
        Model              = vehicle.Model,
        Year               = vehicle.Year,
        VehicleType        = vehicle.VehicleType,
        FuelType           = vehicle.FuelType,
        Transmission       = vehicle.Transmission,
        Color              = vehicle.Color,
        Mileage            = vehicle.Mileage,
        Status             = vehicle.Status,
        PurchaseDate       = vehicle.PurchaseDate,
        PurchasePrice      = vehicle.PurchasePrice,
        RegistrationExpiry = vehicle.RegistrationExpiry,
        CreatedAt          = vehicle.CreatedAt,
        UpdatedAt          = vehicle.UpdatedAt
    };
}
