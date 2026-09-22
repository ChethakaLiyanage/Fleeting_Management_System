using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Inspections;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class InspectionService : IInspectionService
{
    private readonly IFleetDbContext _context;

    public InspectionService(IFleetDbContext context)
    {
        _context = context;
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

        if (filterParams.VehicleId.HasValue)
        {
            query = query.Where(i => i.VehicleId == filterParams.VehicleId.Value);
        }

        if (filterParams.DriverId.HasValue)
        {
            query = query.Where(i => i.DriverId == filterParams.DriverId.Value);
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

        return inspection == null ? null : MapToDto(inspection);
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
            ?? throw new InvalidOperationException($"Vehicle with ID '{dto.VehicleId}' was not found.");

        Driver? driver = null;
        if (dto.DriverId.HasValue)
        {
            driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == dto.DriverId.Value && !d.IsDeleted, cancellationToken);
        }

        Trip? trip = null;
        if (dto.TripId.HasValue)
        {
            trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == dto.TripId.Value && !t.IsDeleted, cancellationToken);
        }

        var inspection = new Inspection
        {
            VehicleId = dto.VehicleId,
            Vehicle = vehicle,
            DriverId = dto.DriverId,
            Driver = driver,
            TripId = dto.TripId,
            Trip = trip,
            Type = dto.Type,
            InspectionDate = dto.InspectionDate ?? DateTime.UtcNow,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var hasFailures = false;
        var hasAttention = false;

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Status == InspectionItemStatus.Fail) hasFailures = true;
            if (itemDto.Status == InspectionItemStatus.Attention) hasAttention = true;

            inspection.Items.Add(new InspectionItem
            {
                ItemName = itemDto.ItemName.Trim(),
                Status = itemDto.Status,
                Notes = itemDto.Notes?.Trim()
            });
        }

        if (hasFailures)
        {
            inspection.Result = InspectionResult.Failed;
            vehicle.Status = VehicleStatus.Maintenance;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }
        else if (hasAttention)
        {
            inspection.Result = InspectionResult.NeedsAttention;
        }
        else
        {
            inspection.Result = InspectionResult.Passed;
        }

        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(inspection);
    }

    private static InspectionDto MapToDto(Inspection i) => new()
    {
        Id = i.Id,
        VehicleId = i.VehicleId,
        VehicleRegistration = i.Vehicle?.RegistrationNumber ?? string.Empty,
        VehicleMakeModel = i.Vehicle != null ? $"{i.Vehicle.Make} {i.Vehicle.Model}" : string.Empty,
        DriverId = i.DriverId,
        DriverName = i.Driver?.FullName,
        TripId = i.TripId,
        TripNumber = i.Trip?.TripNumber,
        Type = i.Type,
        InspectionDate = i.InspectionDate,
        Result = i.Result,
        Notes = i.Notes,
        CreatedAt = i.CreatedAt,
        Items = i.Items.Select(it => new InspectionItemDto
        {
            Id = it.Id,
            ItemName = it.ItemName,
            Status = it.Status,
            Notes = it.Notes
        }).ToList()
    };
}
