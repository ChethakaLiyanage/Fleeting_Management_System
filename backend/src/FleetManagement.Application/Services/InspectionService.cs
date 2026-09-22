using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Inspections;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class InspectionService : IInspectionService
{
    private readonly IFleetDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(
        IFleetDbContext context,
        ICurrentUser currentUser,
        IDashboardCache dashboardCache,
        ILogger<InspectionService> logger)
    {
        _context        = context;
        _currentUser    = currentUser;
        _dashboardCache = dashboardCache;
        _logger         = logger;
    }

    public async Task<PagedResult<InspectionDto>> GetInspectionsAsync(InspectionFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Inspections
            .AsNoTracking()
            .Where(i => !i.IsDeleted);

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (driver == null)
            {
                return new PagedResult<InspectionDto>(new List<InspectionDto>(), 0, filterParams.PageNumber, filterParams.PageSize);
            }

            query = query.Where(i => i.DriverId == driver.Id);
        }
        else if (filterParams.DriverId.HasValue)
        {
            query = query.Where(i => i.DriverId == filterParams.DriverId.Value);
        }

        if (filterParams.VehicleId.HasValue)
        {
            query = query.Where(i => i.VehicleId == filterParams.VehicleId.Value);
        }

        if (filterParams.Type.HasValue)
        {
            query = query.Where(i => i.Type == filterParams.Type.Value);
        }

        if (filterParams.Result.HasValue)
        {
            query = query.Where(i => i.Result == filterParams.Result.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.InspectionDate)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(i => new InspectionDto
            {
                Id                       = i.Id,
                VehicleId                = i.VehicleId,
                VehicleRegistrationNumber = i.Vehicle.RegistrationNumber,
                VehicleMakeModel         = i.Vehicle.Make + " " + i.Vehicle.Model,
                DriverId                 = i.DriverId,
                DriverName               = i.Driver == null ? null : i.Driver.FirstName + " " + i.Driver.LastName,
                TripId                   = i.TripId,
                TripNumber               = i.Trip == null ? null : i.Trip.TripNumber,
                Type                     = i.Type == InspectionType.PreTrip ? "Pre-Trip" :
                                           i.Type == InspectionType.PostTrip ? "Post-Trip" : "Scheduled",
                InspectionDate           = i.InspectionDate,
                Result                   = i.Result == InspectionResult.NeedsAttention ? "Needs Attention" :
                                           i.Result == InspectionResult.Passed ? "Passed" : "Failed",
                Notes                    = i.Notes,
                CreatedAt                = i.CreatedAt,
                Items                    = i.Items.Select(item => new InspectionItemDto
                {
                    Id       = item.Id,
                    ItemName = item.ItemName,
                    Status   = item.Status,
                    Notes    = item.Notes
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<InspectionDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<InspectionDto?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inspection = await _context.Inspections
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .Include(i => i.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (inspection == null) return null;

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (inspection.DriverId != driver?.Id)
            {
                throw new ForbiddenException("You can only access your own inspections.");
            }
        }

        return MapToDto(inspection);
    }

    public async Task<InspectionDto?> UpdateInspectionAsync(Guid id, UpdateInspectionDto dto, CancellationToken cancellationToken = default)
    {
        var inspection = await _context.Inspections
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
        if (inspection == null) return null;

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId && !v.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Vehicle with ID '{dto.VehicleId}' was not found.");
        Driver? driver = null;
        if (dto.DriverId.HasValue)
        {
            driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == dto.DriverId.Value && !d.IsDeleted, cancellationToken)
                ?? throw new NotFoundException($"Driver with ID '{dto.DriverId}' was not found.");
        }
        Trip? trip = null;
        if (dto.TripId.HasValue)
        {
            trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == dto.TripId.Value && !t.IsDeleted, cancellationToken)
                ?? throw new NotFoundException($"Trip with ID '{dto.TripId}' was not found.");
        }

        inspection.VehicleId = dto.VehicleId;
        inspection.Vehicle = vehicle;
        inspection.DriverId = dto.DriverId;
        inspection.Driver = driver;
        inspection.TripId = dto.TripId;
        inspection.Trip = trip;
        inspection.Type = dto.Type;
        inspection.InspectionDate = dto.InspectionDate;
        inspection.Result = dto.Result;
        inspection.Notes = dto.Notes?.Trim();

        var incomingIds = dto.Items.Where(item => item.Id.HasValue).Select(item => item.Id!.Value).ToHashSet();
        foreach (var existing in inspection.Items.Where(item => !incomingIds.Contains(item.Id)).ToList())
            inspection.Items.Remove(existing);
        foreach (var itemDto in dto.Items)
        {
            var item = itemDto.Id.HasValue ? inspection.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value) : null;
            if (item == null)
            {
                item = new InspectionItem();
                inspection.Items.Add(item);
            }
            item.ItemName = itemDto.ItemName.Trim();
            item.Status = itemDto.Status;
            item.Notes = itemDto.Notes?.Trim();
        }
        inspection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _dashboardCache.Invalidate();
        return MapToDto(inspection);
    }

    public async Task<bool> DeleteInspectionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
        if (inspection == null) return false;
        inspection.IsDeleted = true;
        inspection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _dashboardCache.Invalidate();
        _logger.LogWarning("Inspection deleted: {Id}", id);
        return true;
    }

    public async Task<List<InspectionDto>> GetFailedInspectionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inspections
            .AsNoTracking()
            .Where(i => i.Result == InspectionResult.Failed && !i.IsDeleted)
            .OrderByDescending(i => i.InspectionDate)
            .Select(i => new InspectionDto
            {
                Id                       = i.Id,
                VehicleId                = i.VehicleId,
                VehicleRegistrationNumber = i.Vehicle.RegistrationNumber,
                VehicleMakeModel         = i.Vehicle.Make + " " + i.Vehicle.Model,
                DriverId                 = i.DriverId,
                DriverName               = i.Driver == null ? null : i.Driver.FirstName + " " + i.Driver.LastName,
                TripId                   = i.TripId,
                TripNumber               = i.Trip == null ? null : i.Trip.TripNumber,
                Type                     = i.Type == InspectionType.PreTrip ? "Pre-Trip" :
                                           i.Type == InspectionType.PostTrip ? "Post-Trip" : "Scheduled",
                InspectionDate           = i.InspectionDate,
                Result                   = i.Result == InspectionResult.NeedsAttention ? "Needs Attention" :
                                           i.Result == InspectionResult.Passed ? "Passed" : "Failed",
                Notes                    = i.Notes,
                CreatedAt                = i.CreatedAt,
                Items                    = i.Items.Select(item => new InspectionItemDto
                {
                    Id       = item.Id,
                    ItemName = item.ItemName,
                    Status   = item.Status,
                    Notes    = item.Notes
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InspectionDto> CreateInspectionAsync(CreateInspectionDto dto, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && !v.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Vehicle with ID '{dto.VehicleId}' was not found.");

        Guid? driverId = dto.DriverId;
        if (_currentUser.IsInRole("Driver"))
        {
            var driverRecord = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);
            if (driverRecord != null)
            {
                driverId = driverRecord.Id;
            }
        }

        Driver? driver = null;
        if (driverId.HasValue)
        {
            driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == driverId.Value && !d.IsDeleted, cancellationToken);
        }

        Trip? trip = null;
        if (dto.TripId.HasValue)
        {
            trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == dto.TripId.Value && !t.IsDeleted, cancellationToken);
        }

        var hasFailures = dto.Items.Any(item => item.Status == InspectionItemStatus.Fail);
        var hasAttention = dto.Items.Any(item => item.Status == InspectionItemStatus.Attention);

        var overallResult = hasFailures ? InspectionResult.Failed :
                            hasAttention ? InspectionResult.NeedsAttention : InspectionResult.Passed;

        var inspection = new Inspection
        {
            VehicleId      = dto.VehicleId,
            Vehicle        = vehicle,
            DriverId       = driverId,
            Driver         = driver,
            TripId         = dto.TripId,
            Trip           = trip,
            Type           = dto.Type,
            InspectionDate = dto.InspectionDate ?? DateTime.UtcNow,
            Result         = overallResult,
            Notes          = dto.Notes?.Trim(),
            CreatedAt      = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            inspection.Items.Add(new InspectionItem
            {
                ItemName  = itemDto.ItemName.Trim(),
                Status    = itemDto.Status,
                Notes     = itemDto.Notes?.Trim()
            });
        }

        if (hasFailures)
        {
            vehicle.Status = VehicleStatus.Maintenance;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }

        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inspection recorded: {Id} Result: {Result}", inspection.Id, overallResult);
        _dashboardCache.Invalidate();

        return MapToDto(inspection);
    }

    private static InspectionDto MapToDto(Inspection i) => new()
    {
        Id                  = i.Id,
        VehicleId           = i.VehicleId,
        VehicleRegistrationNumber = i.Vehicle?.RegistrationNumber ?? string.Empty,
        VehicleMakeModel    = i.Vehicle != null ? $"{i.Vehicle.Make} {i.Vehicle.Model}" : string.Empty,
        DriverId            = i.DriverId,
        DriverName          = i.Driver?.FullName,
        TripId              = i.TripId,
        TripNumber          = i.Trip?.TripNumber,
        Type                = GetInspectionTypeLabel(i.Type),
        InspectionDate      = i.InspectionDate,
        Result              = GetInspectionResultLabel(i.Result),
        Notes               = i.Notes,
        CreatedAt           = i.CreatedAt,
        Items               = i.Items.Select(item => new InspectionItemDto
        {
            Id       = item.Id,
            ItemName = item.ItemName,
            Status   = item.Status,
            Notes    = item.Notes
        }).ToList()
    };

    private static string GetInspectionTypeLabel(InspectionType type) => type switch
    {
        InspectionType.PreTrip => "Pre-Trip",
        InspectionType.PostTrip => "Post-Trip",
        _ => "Scheduled"
    };

    private static string GetInspectionResultLabel(InspectionResult result) => result switch
    {
        InspectionResult.Passed => "Passed",
        InspectionResult.Failed => "Failed",
        _ => "Needs Attention"
    };
}
