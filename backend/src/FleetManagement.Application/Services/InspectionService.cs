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
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .Include(i => i.Items)
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
            .Select(i => MapToDto(i))
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

    public async Task<List<InspectionDto>> GetFailedInspectionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inspections
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .Include(i => i.Items)
            .AsNoTracking()
            .Where(i => i.Result == InspectionResult.Failed && !i.IsDeleted)
            .OrderByDescending(i => i.InspectionDate)
            .Select(i => MapToDto(i))
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

        var inspectionNumber = $"INS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var overallResult = dto.Items.Any(item => item.Status == InspectionItemStatus.Fail)
            ? InspectionResult.Failed
            : InspectionResult.Passed;

        var inspection = new Inspection
        {
            InspectionNumber = inspectionNumber,
            VehicleId        = dto.VehicleId,
            Vehicle          = vehicle,
            DriverId         = driverId,
            TripId           = dto.TripId,
            InspectionDate   = dto.InspectionDate,
            Type             = dto.Type,
            Result           = overallResult,
            Notes            = dto.Notes?.Trim(),
            CreatedAt        = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            inspection.Items.Add(new InspectionItem
            {
                ItemName  = itemDto.ItemName.Trim(),
                Category  = itemDto.Category.Trim(),
                Status    = itemDto.Status,
                Comments  = itemDto.Comments?.Trim(),
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inspection recorded: {Number} Result: {Result}", inspection.InspectionNumber, overallResult);
        _dashboardCache.Invalidate();

        return MapToDto(inspection);
    }

    private static InspectionDto MapToDto(Inspection i) => new()
    {
        Id                  = i.Id,
        InspectionNumber    = i.InspectionNumber,
        VehicleId           = i.VehicleId,
        VehicleRegistration = i.Vehicle?.RegistrationNumber ?? string.Empty,
        DriverId            = i.DriverId,
        DriverName          = i.Driver?.FullName,
        TripId              = i.TripId,
        TripNumber          = i.Trip?.TripNumber,
        InspectionDate      = i.InspectionDate,
        Type                = i.Type,
        Result              = i.Result,
        Notes               = i.Notes,
        CreatedAt           = i.CreatedAt,
        Items               = i.Items.Select(item => new InspectionItemDto
        {
            Id        = item.Id,
            ItemName  = item.ItemName,
            Category  = item.Category,
            Status    = item.Status,
            Comments  = item.Comments,
            CreatedAt = item.CreatedAt
        }).ToList()
    };
}
