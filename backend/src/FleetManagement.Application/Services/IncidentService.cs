using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Incidents;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class IncidentService : IIncidentService
{
    private readonly IFleetDbContext _context;

    public IncidentService(IFleetDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<IncidentDto>> GetIncidentsAsync(IncidentFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Incidents
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
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

        if (filterParams.Severity.HasValue)
        {
            query = query.Where(i => i.Severity == filterParams.Severity.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(i => i.Status == filterParams.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.Date)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(i => MapToDto(i))
            .ToListAsync(cancellationToken);

        return new PagedResult<IncidentDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<IncidentDto?> GetIncidentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incident = await _context.Incidents
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        return incident == null ? null : MapToDto(incident);
    }

    public async Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, CancellationToken cancellationToken = default)
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

        var incident = new Incident
        {
            VehicleId = dto.VehicleId,
            Vehicle = vehicle,
            DriverId = dto.DriverId,
            Driver = driver,
            TripId = dto.TripId,
            Trip = trip,
            Date = dto.Date ?? DateTime.UtcNow,
            Location = dto.Location.Trim(),
            Type = dto.Type,
            Description = dto.Description.Trim(),
            Severity = dto.Severity,
            PoliceReportNumber = dto.PoliceReportNumber?.Trim(),
            InsuranceClaimNumber = dto.InsuranceClaimNumber?.Trim(),
            EstimatedDamage = dto.EstimatedDamage,
            ActualRepairCost = dto.ActualRepairCost,
            Status = IncidentStatus.Reported,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Severity == IncidentSeverity.Critical || dto.Type is IncidentType.Accident or IncidentType.Breakdown)
        {
            vehicle.Status = VehicleStatus.OutOfService;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(incident);
    }

    public async Task<IncidentDto?> UpdateIncidentAsync(Guid id, UpdateIncidentDto dto, CancellationToken cancellationToken = default)
    {
        var incident = await _context.Incidents
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (incident == null) return null;

        if (dto.Date.HasValue) incident.Date = dto.Date.Value;
        incident.Location = dto.Location.Trim();
        incident.Type = dto.Type;
        incident.Description = dto.Description.Trim();
        incident.Severity = dto.Severity;
        incident.PoliceReportNumber = dto.PoliceReportNumber?.Trim();
        incident.InsuranceClaimNumber = dto.InsuranceClaimNumber?.Trim();
        incident.EstimatedDamage = dto.EstimatedDamage;
        incident.ActualRepairCost = dto.ActualRepairCost;
        incident.Status = dto.Status;
        incident.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(incident);
    }

    public async Task<IncidentDto?> UpdateIncidentStatusAsync(Guid id, UpdateIncidentStatusDto dto, CancellationToken cancellationToken = default)
    {
        var incident = await _context.Incidents
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (incident == null) return null;

        incident.Status = dto.Status;
        if (dto.ActualRepairCost.HasValue)
        {
            incident.ActualRepairCost = dto.ActualRepairCost.Value;
        }
        incident.UpdatedAt = DateTime.UtcNow;

        if (dto.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
        {
            if (incident.Vehicle.Status == VehicleStatus.OutOfService)
            {
                incident.Vehicle.Status = VehicleStatus.Available;
                incident.Vehicle.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(incident);
    }

    private static IncidentDto MapToDto(Incident i) => new()
    {
        Id = i.Id,
        VehicleId = i.VehicleId,
        VehicleRegistration = i.Vehicle?.RegistrationNumber ?? string.Empty,
        VehicleMakeModel = i.Vehicle != null ? $"{i.Vehicle.Make} {i.Vehicle.Model}" : string.Empty,
        DriverId = i.DriverId,
        DriverName = i.Driver?.FullName,
        TripId = i.TripId,
        TripNumber = i.Trip?.TripNumber,
        Date = i.Date,
        Location = i.Location,
        Type = i.Type,
        Description = i.Description,
        Severity = i.Severity,
        PoliceReportNumber = i.PoliceReportNumber,
        InsuranceClaimNumber = i.InsuranceClaimNumber,
        EstimatedDamage = i.EstimatedDamage,
        ActualRepairCost = i.ActualRepairCost,
        Status = i.Status,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };
}
