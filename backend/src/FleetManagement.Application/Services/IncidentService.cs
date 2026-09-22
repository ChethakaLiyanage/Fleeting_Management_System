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
            .Select(i => new IncidentDto
            {
                Id                  = i.Id,
                IncidentNumber      = i.IncidentNumber,
                VehicleId           = i.VehicleId,
                VehicleRegistration = i.Vehicle != null ? i.Vehicle.RegistrationNumber : string.Empty,
                DriverId            = i.DriverId,
                DriverName          = i.Driver != null ? (i.Driver.FirstName + " " + i.Driver.LastName) : null,
                TripId              = i.TripId,
                TripNumber          = i.Trip != null ? i.Trip.TripNumber : null,
                Date                = i.Date,
                Type                = i.Type,
                Severity            = i.Severity,
                Description         = i.Description,
                Location            = i.Location,
                EstimatedCost       = i.EstimatedCost,
                ActualCost          = i.ActualCost,
                PoliceReportNumber  = i.PoliceReportNumber,
                Status              = i.Status,
                CreatedAt           = i.CreatedAt,
                UpdatedAt           = i.UpdatedAt
            })
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

        var incidentNumber = $"INC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var incident = new Incident
        {
            IncidentNumber     = incidentNumber,
            VehicleId          = dto.VehicleId,
            Vehicle            = vehicle,
            DriverId           = driverId,
            TripId             = dto.TripId,
            Date               = dto.Date,
            Type               = dto.Type,
            Severity           = dto.Severity,
            Description        = dto.Description.Trim(),
            Location           = dto.Location.Trim(),
            EstimatedCost      = dto.EstimatedCost,
            PoliceReportNumber = dto.PoliceReportNumber?.Trim(),
            Status             = IncidentStatus.Reported,
            CreatedAt          = DateTime.UtcNow
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident reported: {Number} for vehicle {VehicleId}", incident.IncidentNumber, dto.VehicleId);
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

        incident.Date               = dto.Date;
        incident.Type               = dto.Type;
        incident.Severity           = dto.Severity;
        incident.Description        = dto.Description.Trim();
        incident.Location           = dto.Location.Trim();
        incident.EstimatedCost      = dto.EstimatedCost;
        incident.PoliceReportNumber = dto.PoliceReportNumber?.Trim();
        incident.UpdatedAt          = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident updated: {Number}", incident.IncidentNumber);
        _dashboardCache.Invalidate();

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

        incident.Status    = dto.Status;
        incident.UpdatedAt = DateTime.UtcNow;

        if (dto.ActualCost.HasValue)
        {
            incident.ActualCost = dto.ActualCost.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Incident {Number} status changed to {Status}", incident.IncidentNumber, dto.Status);
        _dashboardCache.Invalidate();

        return MapToDto(incident);
    }

    private static IncidentDto MapToDto(Incident i) => new()
    {
        Id                  = i.Id,
        IncidentNumber      = i.IncidentNumber,
        VehicleId           = i.VehicleId,
        VehicleRegistration = i.Vehicle?.RegistrationNumber ?? string.Empty,
        DriverId            = i.DriverId,
        DriverName          = i.Driver?.FullName,
        TripId              = i.TripId,
        TripNumber          = i.Trip?.TripNumber,
        Date                = i.Date,
        Type                = i.Type,
        Severity            = i.Severity,
        Description         = i.Description,
        Location            = i.Location,
        EstimatedCost       = i.EstimatedCost,
        ActualCost          = i.ActualCost,
        PoliceReportNumber  = i.PoliceReportNumber,
        Status              = i.Status,
        CreatedAt           = i.CreatedAt,
        UpdatedAt           = i.UpdatedAt
    };
}
