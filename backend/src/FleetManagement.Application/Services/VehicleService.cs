using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Vehicles;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IFleetDbContext _context;

    public VehicleService(IFleetDbContext context)
    {
        _context = context;
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

        var items = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(v => MapToDto(v))
            .ToListAsync(cancellationToken);

        return new PagedResult<VehicleDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

        return vehicle == null ? null : MapToDto(vehicle);
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
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Status = vehicle.Status,
            CurrentMileage = vehicle.Mileage,
            RegistrationExpiry = vehicle.RegistrationExpiry,
            IsRegistrationExpiringSoon = isExpiringSoon,
            TotalTrips = 0,
            TotalAssignments = 0,
            TotalInspections = 0,
            TotalIncidents = 0
        };
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Vehicles
            .AnyAsync(v => v.RegistrationNumber.ToLower() == dto.RegistrationNumber.ToLower() && !v.IsDeleted, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Vehicle with registration number '{dto.RegistrationNumber}' already exists.");
        }

        var vehicle = new Vehicle
        {
            RegistrationNumber = dto.RegistrationNumber.Trim().ToUpper(),
            VIN = dto.VIN?.Trim().ToUpper(),
            EngineNumber = dto.EngineNumber.Trim(),
            Make = dto.Make.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            VehicleType = dto.VehicleType,
            FuelType = dto.FuelType,
            Transmission = dto.Transmission,
            Color = dto.Color.Trim(),
            Mileage = dto.Mileage,
            Status = VehicleStatus.Available,
            PurchaseDate = dto.PurchaseDate,
            PurchasePrice = dto.PurchasePrice,
            RegistrationExpiry = dto.RegistrationExpiry,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

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
            throw new InvalidOperationException($"Another vehicle with registration number '{dto.RegistrationNumber}' already exists.");
        }

        vehicle.RegistrationNumber = dto.RegistrationNumber.Trim().ToUpper();
        vehicle.VIN = dto.VIN?.Trim().ToUpper();
        vehicle.EngineNumber = dto.EngineNumber.Trim();
        vehicle.Make = dto.Make.Trim();
        vehicle.Model = dto.Model.Trim();
        vehicle.Year = dto.Year;
        vehicle.VehicleType = dto.VehicleType;
        vehicle.FuelType = dto.FuelType;
        vehicle.Transmission = dto.Transmission;
        vehicle.Color = dto.Color.Trim();
        vehicle.Mileage = dto.Mileage;
        vehicle.Status = dto.Status;
        vehicle.PurchaseDate = dto.PurchaseDate;
        vehicle.PurchasePrice = dto.PurchasePrice;
        vehicle.RegistrationExpiry = dto.RegistrationExpiry;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(vehicle);
    }

    public async Task<bool> ArchiveVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

        if (vehicle == null) return false;

        vehicle.Status = VehicleStatus.Retired;
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static VehicleDto MapToDto(Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        RegistrationNumber = vehicle.RegistrationNumber,
        VIN = vehicle.VIN,
        EngineNumber = vehicle.EngineNumber,
        Make = vehicle.Make,
        Model = vehicle.Model,
        Year = vehicle.Year,
        VehicleType = vehicle.VehicleType,
        FuelType = vehicle.FuelType,
        Transmission = vehicle.Transmission,
        Color = vehicle.Color,
        Mileage = vehicle.Mileage,
        Status = vehicle.Status,
        PurchaseDate = vehicle.PurchaseDate,
        PurchasePrice = vehicle.PurchasePrice,
        RegistrationExpiry = vehicle.RegistrationExpiry,
        CreatedAt = vehicle.CreatedAt,
        UpdatedAt = vehicle.UpdatedAt
    };
}
