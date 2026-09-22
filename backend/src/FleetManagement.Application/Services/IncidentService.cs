using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Incidents;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class IncidentService : IIncidentService
{
    private readonly IFleetDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<IncidentService> _logger;

    public IncidentService(
        IFleetDbContext context,
        ICurrentUser currentUser,
        IDashboardCache dashboardCache,
        ILogger<IncidentService> logger)
    {
        _context        = context;
        _currentUser    = currentUser;
        _dashboardCache = dashboardCache;
        _logger         = logger;
    }

    public async Task<PagedResult<IncidentDto>> GetIncidentsAsync(IncidentFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Incidents
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .AsNoTracking()
            .Where(i => !i.IsDeleted);

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (driver == null)
            {
                return new PagedResult<IncidentDto>(new List<IncidentDto>(), 0, filterParams.PageNumber, filterParams.PageSize);
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

        if (incident == null) return null;

        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (incident.DriverId != driver?.Id)
            {
                throw new ForbiddenException("You can only access your own incidents.");
            }
        }

        return MapToDto(incident);
    }

    public async Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, CancellationToken cancellationToken = default)
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

        var incident = new Incident
        {
            VehicleId            = dto.VehicleId,
            Vehicle              = vehicle,
            DriverId             = driverId,
            TripId               = dto.TripId,
            Date                 = dto.Date ?? DateTime.UtcNow,
            Location             = dto.Location.Trim(),
            Type                 = dto.Type,
            Description          = dto.Description.Trim(),
            Severity             = dto.Severity,
            PoliceReportNumber   = dto.PoliceReportNumber?.Trim(),
            InsuranceClaimNumber = dto.InsuranceClaimNumber?.Trim(),
            EstimatedDamage      = dto.EstimatedDamage,
            ActualRepairCost     = dto.ActualRepairCost,
            Status               = IncidentStatus.Reported,
            CreatedAt            = DateTime.UtcNow
        };

        if (dto.Severity == IncidentSeverity.Critical || dto.Type is IncidentType.Accident or IncidentType.Breakdown)
        {
            vehicle.Status = VehicleStatus.OutOfService;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident reported: {Id} for vehicle {VehicleId}", incident.Id, dto.VehicleId);
        _dashboardCache.Invalidate();

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
        incident.Location             = dto.Location.Trim();
        incident.Type                 = dto.Type;
        incident.Description          = dto.Description.Trim();
        incident.Severity             = dto.Severity;
        incident.PoliceReportNumber   = dto.PoliceReportNumber?.Trim();
        incident.InsuranceClaimNumber = dto.InsuranceClaimNumber?.Trim();
        incident.EstimatedDamage      = dto.EstimatedDamage;
        incident.ActualRepairCost     = dto.ActualRepairCost;
        incident.Status               = dto.Status;
        incident.UpdatedAt          = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident updated: {Id}", incident.Id);
        _dashboardCache.Invalidate();

        return MapToDto(incident);
    }

    public async Task<bool> DeleteIncidentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incident = await _context.Incidents.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
        if (incident == null) return false;
        incident.IsDeleted = true;
        incident.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _dashboardCache.Invalidate();
        _logger.LogWarning("Incident deleted: {Id}", id);
        return true;
    }

    public async Task<IncidentDto?> UpdateIncidentStatusAsync(Guid id, UpdateIncidentStatusDto dto, CancellationToken cancellationToken = default)
    {
        var incident = await _context.Incidents
            .Include(i => i.Vehicle)
            .Include(i => i.Driver)
            .Include(i => i.Trip)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (incident == null) return null;

        incident.Status    = dto.Status;
        incident.UpdatedAt = DateTime.UtcNow;

        if (dto.ActualRepairCost.HasValue)
        {
            incident.ActualRepairCost = dto.ActualRepairCost.Value;
        }

        if (dto.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
        {
            if (incident.Vehicle != null && incident.Vehicle.Status == VehicleStatus.OutOfService)
            {
                incident.Vehicle.Status = VehicleStatus.Available;
                incident.Vehicle.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident {Id} status changed to {Status}", incident.Id, dto.Status);
        _dashboardCache.Invalidate();

        return MapToDto(incident);
    }

    private static IncidentDto MapToDto(Incident i) => new()
    {
        Id                   = i.Id,
        VehicleId            = i.VehicleId,
        VehicleRegistration  = i.Vehicle?.RegistrationNumber ?? string.Empty,
        VehicleMakeModel     = i.Vehicle != null ? $"{i.Vehicle.Make} {i.Vehicle.Model}" : string.Empty,
        DriverId             = i.DriverId,
        DriverName           = i.Driver != null ? $"{i.Driver.FirstName} {i.Driver.LastName}" : null,
        TripId               = i.TripId,
        TripNumber           = i.Trip?.TripNumber,
        Date                 = i.Date,
        Location             = i.Location,
        Type                 = i.Type,
        Description          = i.Description,
        Severity             = i.Severity,
        PoliceReportNumber   = i.PoliceReportNumber,
        InsuranceClaimNumber = i.InsuranceClaimNumber,
        EstimatedDamage      = i.EstimatedDamage,
        ActualRepairCost     = i.ActualRepairCost,
        Status               = i.Status,
        CreatedAt            = i.CreatedAt,
        UpdatedAt            = i.UpdatedAt
    };
}
