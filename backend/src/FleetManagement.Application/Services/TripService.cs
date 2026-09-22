using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Trips;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class TripService : ITripService
{
    private readonly IFleetDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<TripService> _logger;

    public TripService(
        IFleetDbContext context,
        ICurrentUser currentUser,
        IDashboardCache dashboardCache,
        ILogger<TripService> logger)
    {
        _context        = context;
        _currentUser    = currentUser;
        _dashboardCache = dashboardCache;
        _logger         = logger;
    }

    public async Task<PagedResult<TripDto>> GetTripsAsync(TripFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Trips
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        // RBAC resource ownership: Driver can only view their own trips
        if (_currentUser.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (driver == null)
            {
                return new PagedResult<TripDto>(new List<TripDto>(), 0, filterParams.PageNumber, filterParams.PageSize);
            }

            query = query.Where(t => t.DriverId == driver.Id);
        }
        else if (filterParams.DriverId.HasValue)
        {
            query = query.Where(t => t.DriverId == filterParams.DriverId.Value);
        }

        if (filterParams.VehicleId.HasValue)
        {
            query = query.Where(t => t.VehicleId == filterParams.VehicleId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(t => t.Status == filterParams.Status.Value);
        }

        if (filterParams.FromDate.HasValue)
        {
            query = query.Where(t => t.StartTime >= filterParams.FromDate.Value || t.CreatedAt >= filterParams.FromDate.Value);
        }

        if (filterParams.ToDate.HasValue)
        {
            query = query.Where(t => t.StartTime <= filterParams.ToDate.Value || t.CreatedAt <= filterParams.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Direct SQL projection
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(t => new TripDto
            {
                Id                  = t.Id,
                TripNumber          = t.TripNumber,
                VehicleId           = t.VehicleId,
                VehicleRegistration = t.Vehicle != null ? t.Vehicle.RegistrationNumber : string.Empty,
                VehicleMakeModel    = t.Vehicle != null ? (t.Vehicle.Make + " " + t.Vehicle.Model) : string.Empty,
                DriverId            = t.DriverId,
                DriverName          = t.Driver != null ? (t.Driver.FirstName + " " + t.Driver.LastName) : string.Empty,
                StartLocation       = t.StartLocation,
                Destination         = t.Destination,
                StartTime           = t.StartTime,
                EndTime             = t.EndTime,
                StartingMileage     = t.StartingMileage,
                EndingMileage       = t.EndingMileage,
                Distance            = t.Distance,
                Purpose             = t.Purpose,
                Status              = t.Status,
                Notes               = t.Notes,
                CreatedAt           = t.CreatedAt,
                UpdatedAt           = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TripDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<TripDto?> GetTripByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (trip == null) return null;

        // Resource ownership check for drivers
        if (_currentUser.IsInRole("Driver"))
        {
            var driverRecord = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (trip.DriverId != driverRecord?.Id)
            {
                throw new ForbiddenException("You can only access your own trips.");
            }
        }

        return MapToDto(trip);
    }

    public async Task<TripDto> CreateTripAsync(CreateTripDto dto, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && !v.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Vehicle with ID '{dto.VehicleId}' was not found.");

        if (vehicle.Status is VehicleStatus.Maintenance or VehicleStatus.OutOfService or VehicleStatus.Retired)
        {
            throw new InvalidOperationException($"Vehicle '{vehicle.RegistrationNumber}' cannot be assigned to a trip because it is {vehicle.Status}.");
        }

        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == dto.DriverId && !d.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Driver with ID '{dto.DriverId}' was not found.");

        if (driver.Status is DriverStatus.Inactive or DriverStatus.Suspended)
        {
            throw new InvalidOperationException($"Driver '{driver.FullName}' cannot be assigned to a trip because their status is {driver.Status}.");
        }

        if (driver.IsLicenseExpired)
        {
            throw new InvalidOperationException($"Driver '{driver.FullName}' cannot be assigned to a trip because their driving license is expired.");
        }

        var activeTripExists = await _context.Trips
            .AnyAsync(t => (t.VehicleId == dto.VehicleId || t.DriverId == dto.DriverId) &&
                           t.Status == TripStatus.InProgress &&
                           !t.IsDeleted, cancellationToken);

        if (activeTripExists)
        {
            throw new InvalidOperationException("Cannot schedule or assign trip: vehicle or driver is already on an active In-Progress trip.");
        }

        var tripNumber = $"TRP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var trip = new Trip
        {
            TripNumber      = tripNumber,
            VehicleId       = dto.VehicleId,
            Vehicle         = vehicle,
            DriverId        = dto.DriverId,
            Driver          = driver,
            StartLocation   = dto.StartLocation.Trim(),
            Destination     = dto.Destination.Trim(),
            StartTime       = dto.ScheduledStartTime,
            Purpose         = dto.Purpose.Trim(),
            Status          = TripStatus.Scheduled,
            StartingMileage = vehicle.Mileage,
            Notes           = dto.Notes?.Trim(),
            CreatedAt       = DateTime.UtcNow
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trip {Number} created", trip.TripNumber);
        _dashboardCache.Invalidate();

        return MapToDto(trip);
    }

    public async Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto dto, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (trip == null) return null;

        if (trip.Status is TripStatus.Completed or TripStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot modify a trip that is already {trip.Status}.");
        }

        trip.StartLocation = dto.StartLocation.Trim();
        trip.Destination   = dto.Destination.Trim();
        trip.Purpose       = dto.Purpose.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            trip.Notes = dto.Notes.Trim();
        }
        trip.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trip {Number} updated", trip.TripNumber);
        _dashboardCache.Invalidate();

        return MapToDto(trip);
    }

    public async Task<TripDto> StartTripAsync(Guid id, StartTripDto dto, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Trip), id);

        // Driver ownership check
        if (_currentUser.IsInRole("Driver"))
        {
            var driverRecord = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (trip.DriverId != driverRecord?.Id)
            {
                throw new ForbiddenException("You can only start your own trips.");
            }
        }

        if (trip.Status != TripStatus.Scheduled && trip.Status != TripStatus.Assigned)
        {
            throw new InvalidOperationException($"Only Scheduled or Assigned trips can be started. Current status: {trip.Status}.");
        }

        var startMileage = dto.StartingMileage ?? trip.Vehicle.Mileage;
        trip.StartingMileage = startMileage;
        trip.StartTime       = dto.StartTime ?? DateTime.UtcNow;
        trip.Status          = TripStatus.InProgress;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            trip.Notes = string.IsNullOrWhiteSpace(trip.Notes) ? dto.Notes.Trim() : $"{trip.Notes} | Start: {dto.Notes.Trim()}";
        }
        trip.UpdatedAt = DateTime.UtcNow;

        trip.Vehicle.Status    = VehicleStatus.OnTrip;
        trip.Vehicle.UpdatedAt = DateTime.UtcNow;

        trip.Driver.Status    = DriverStatus.OnTrip;
        trip.Driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trip {Number} started", trip.TripNumber);
        _dashboardCache.Invalidate();

        return MapToDto(trip);
    }

    public async Task<TripDto> CompleteTripAsync(Guid id, CompleteTripDto dto, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Trip), id);

        // Driver ownership check
        if (_currentUser.IsInRole("Driver"))
        {
            var driverRecord = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);

            if (trip.DriverId != driverRecord?.Id)
            {
                throw new ForbiddenException("You can only complete your own trips.");
            }
        }

        if (trip.Status != TripStatus.InProgress)
        {
            throw new InvalidOperationException($"Trip cannot be completed because its current status is {trip.Status}.");
        }

        var startMileage = trip.StartingMileage ?? trip.Vehicle.Mileage;
        if (dto.EndingMileage < startMileage)
        {
            throw new InvalidOperationException($"Ending mileage ({dto.EndingMileage}) cannot be lower than starting mileage ({startMileage}).");
        }

        var distance = dto.EndingMileage - startMileage;
        trip.EndingMileage = dto.EndingMileage;
        trip.Distance      = distance;
        trip.EndTime       = dto.EndTime ?? DateTime.UtcNow;
        trip.Status        = TripStatus.Completed;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            trip.Notes = string.IsNullOrWhiteSpace(trip.Notes) ? dto.Notes.Trim() : $"{trip.Notes} | Completed: {dto.Notes.Trim()}";
        }
        trip.UpdatedAt = DateTime.UtcNow;

        trip.Vehicle.Mileage = dto.EndingMileage;
        await RestoreAvailabilityAsync(trip.Vehicle, trip.Driver, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trip {Number} completed. Distance: {Km}km", trip.TripNumber, distance);
        _dashboardCache.Invalidate();

        return MapToDto(trip);
    }

    public async Task<TripDto> CancelTripAsync(Guid id, string? reason, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Trip), id);

        if (trip.Status == TripStatus.Completed)
        {
            throw new InvalidOperationException("Completed trips cannot be cancelled.");
        }

        trip.Status = TripStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            trip.Notes = string.IsNullOrWhiteSpace(trip.Notes) ? $"Cancelled: {reason.Trim()}" : $"{trip.Notes} | Cancelled: {reason.Trim()}";
        }
        trip.UpdatedAt = DateTime.UtcNow;

        await RestoreAvailabilityAsync(trip.Vehicle, trip.Driver, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Trip {Number} cancelled. Reason: {Reason}", trip.TripNumber, reason ?? "none");
        _dashboardCache.Invalidate();

        return MapToDto(trip);
    }

    private async Task RestoreAvailabilityAsync(Vehicle vehicle, Driver driver, CancellationToken cancellationToken)
    {
        var vehicleHasActiveAssignment = await _context.VehicleAssignments
            .AnyAsync(a => a.VehicleId == vehicle.Id && a.Status == AssignmentStatus.Active && !a.IsDeleted, cancellationToken);

        if (vehicle.Status == VehicleStatus.OnTrip)
        {
            vehicle.Status    = vehicleHasActiveAssignment ? VehicleStatus.Assigned : VehicleStatus.Available;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }

        var driverHasActiveAssignment = await _context.VehicleAssignments
            .AnyAsync(a => a.DriverId == driver.Id && a.Status == AssignmentStatus.Active && !a.IsDeleted, cancellationToken);

        if (driver.Status == DriverStatus.OnTrip)
        {
            driver.Status    = driverHasActiveAssignment ? DriverStatus.Assigned : DriverStatus.Available;
            driver.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static TripDto MapToDto(Trip t) => new()
    {
        Id                  = t.Id,
        TripNumber          = t.TripNumber,
        VehicleId           = t.VehicleId,
        VehicleRegistration = t.Vehicle?.RegistrationNumber ?? string.Empty,
        VehicleMakeModel    = t.Vehicle != null ? $"{t.Vehicle.Make} {t.Vehicle.Model}" : string.Empty,
        DriverId            = t.DriverId,
        DriverName          = t.Driver?.FullName ?? string.Empty,
        StartLocation       = t.StartLocation,
        Destination         = t.Destination,
        StartTime           = t.StartTime,
        EndTime             = t.EndTime,
        StartingMileage     = t.StartingMileage,
        EndingMileage       = t.EndingMileage,
        Distance            = t.Distance,
        Purpose             = t.Purpose,
        Status              = t.Status,
        Notes               = t.Notes,
        CreatedAt           = t.CreatedAt,
        UpdatedAt           = t.UpdatedAt
    };
}
