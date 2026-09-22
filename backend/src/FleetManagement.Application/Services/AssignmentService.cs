using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Assignments;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IFleetDbContext _context;

    public AssignmentService(IFleetDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AssignmentDto>> GetAssignmentsAsync(AssignmentFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleAssignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (filterParams.VehicleId.HasValue)
        {
            query = query.Where(a => a.VehicleId == filterParams.VehicleId.Value);
        }

        if (filterParams.DriverId.HasValue)
        {
            query = query.Where(a => a.DriverId == filterParams.DriverId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(a => a.Status == filterParams.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.AssignedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(a => MapToDto(a))
            .ToListAsync(cancellationToken);

        return new PagedResult<AssignmentDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<List<AssignmentDto>> GetActiveAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.VehicleAssignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .AsNoTracking()
            .Where(a => a.Status == AssignmentStatus.Active && !a.IsDeleted)
            .OrderByDescending(a => a.AssignedAt)
            .Select(a => MapToDto(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssignmentDto> AssignVehicleAsync(CreateAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && !v.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with ID '{dto.VehicleId}' was not found.");

        if (vehicle.Status is VehicleStatus.Maintenance or VehicleStatus.OutOfService or VehicleStatus.Retired)
        {
            throw new InvalidOperationException($"Vehicle cannot be assigned because its status is {vehicle.Status}.");
        }

        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == dto.DriverId && !d.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Driver with ID '{dto.DriverId}' was not found.");

        if (driver.Status is DriverStatus.Inactive or DriverStatus.Suspended)
        {
            throw new InvalidOperationException($"Driver cannot be assigned because their status is {driver.Status}.");
        }

        if (driver.IsLicenseExpired)
        {
            throw new InvalidOperationException("Driver cannot be assigned because their driving license is expired.");
        }

        var activeVehicleAssignment = await _context.VehicleAssignments
            .AnyAsync(a => a.VehicleId == dto.VehicleId && a.Status == AssignmentStatus.Active && !a.IsDeleted, cancellationToken);
        if (activeVehicleAssignment)
        {
            throw new InvalidOperationException($"Vehicle '{vehicle.RegistrationNumber}' already has an active assignment.");
        }

        var activeDriverAssignment = await _context.VehicleAssignments
            .AnyAsync(a => a.DriverId == dto.DriverId && a.Status == AssignmentStatus.Active && !a.IsDeleted, cancellationToken);
        if (activeDriverAssignment)
        {
            throw new InvalidOperationException($"Driver '{driver.FullName}' already has an active vehicle assignment.");
        }

        var assignment = new VehicleAssignment
        {
            VehicleId = dto.VehicleId,
            Vehicle = vehicle,
            DriverId = dto.DriverId,
            Driver = driver,
            AssignedAt = dto.AssignedAt ?? DateTime.UtcNow,
            Status = AssignmentStatus.Active,
            AssignedByUserId = dto.AssignedByUserId,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        vehicle.Status = VehicleStatus.Assigned;
        driver.Status = DriverStatus.Assigned;

        _context.VehicleAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(assignment);
    }

    public async Task<AssignmentDto?> EndAssignmentAsync(Guid id, EndAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.VehicleAssignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (assignment == null) return null;

        if (assignment.Status == AssignmentStatus.Ended)
        {
            throw new InvalidOperationException("This assignment has already ended.");
        }

        assignment.Status = AssignmentStatus.Ended;
        assignment.UnassignedAt = dto.UnassignedAt ?? DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            assignment.Notes = string.IsNullOrWhiteSpace(assignment.Notes)
                ? dto.Notes.Trim()
                : $"{assignment.Notes} | Ended: {dto.Notes.Trim()}";
        }
        assignment.UpdatedAt = DateTime.UtcNow;

        if (assignment.Vehicle.Status == VehicleStatus.Assigned)
        {
            assignment.Vehicle.Status = VehicleStatus.Available;
            assignment.Vehicle.UpdatedAt = DateTime.UtcNow;
        }

        if (assignment.Driver.Status == DriverStatus.Assigned)
        {
            assignment.Driver.Status = DriverStatus.Available;
            assignment.Driver.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(assignment);
    }

    private static AssignmentDto MapToDto(VehicleAssignment a) => new()
    {
        Id = a.Id,
        VehicleId = a.VehicleId,
        VehicleRegistration = a.Vehicle?.RegistrationNumber ?? string.Empty,
        VehicleMakeModel = a.Vehicle != null ? $"{a.Vehicle.Make} {a.Vehicle.Model}" : string.Empty,
        DriverId = a.DriverId,
        DriverName = a.Driver?.FullName ?? string.Empty,
        DriverEmployeeNumber = a.Driver?.EmployeeNumber ?? string.Empty,
        AssignedAt = a.AssignedAt,
        UnassignedAt = a.UnassignedAt,
        Status = a.Status,
        AssignedByUserId = a.AssignedByUserId,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt
    };
}
