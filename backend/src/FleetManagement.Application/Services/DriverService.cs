using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Drivers;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class DriverService : IDriverService
{
    private readonly IFleetDbContext _context;

    public DriverService(IFleetDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DriverDto>> GetDriversAsync(DriverFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Drivers
            .AsNoTracking()
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var search = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(d =>
                d.FirstName.ToLower().Contains(search) ||
                d.LastName.ToLower().Contains(search) ||
                d.EmployeeNumber.ToLower().Contains(search) ||
                d.LicenseNumber.ToLower().Contains(search) ||
                d.Phone.Contains(search));
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(d => d.Status == filterParams.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.LicenseClass))
        {
            query = query.Where(d => d.LicenseClass.ToLower() == filterParams.LicenseClass.Trim().ToLower());
        }

        if (filterParams.LicenseExpiredOnly.HasValue && filterParams.LicenseExpiredOnly.Value)
        {
            query = query.Where(d => d.LicenseExpiry <= DateTime.UtcNow);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filterParams.SortBy?.ToLower() switch
        {
            "name" => filterParams.IsDescending ? query.OrderByDescending(d => d.FirstName) : query.OrderBy(d => d.FirstName),
            "employeenumber" => filterParams.IsDescending ? query.OrderByDescending(d => d.EmployeeNumber) : query.OrderBy(d => d.EmployeeNumber),
            "licenseexpiry" => filterParams.IsDescending ? query.OrderByDescending(d => d.LicenseExpiry) : query.OrderBy(d => d.LicenseExpiry),
            _ => filterParams.IsDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        var items = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(d => MapToDto(d))
            .ToListAsync(cancellationToken);

        return new PagedResult<DriverDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<DriverDto?> GetDriverByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        return driver == null ? null : MapToDto(driver);
    }

    public async Task<DriverHistoryDto?> GetDriverHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (driver == null) return null;

        return new DriverHistoryDto
        {
            DriverId = driver.Id,
            FullName = driver.FullName,
            LicenseNumber = driver.LicenseNumber,
            Status = driver.Status,
            TotalTripsCompleted = 0,
            TotalKilometersDriven = 0,
            TotalIncidentsInvolved = 0,
            CurrentlyAssignedVehicleId = null,
            CurrentlyAssignedVehicleReg = null
        };
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverDto dto, CancellationToken cancellationToken = default)
    {
        var empExists = await _context.Drivers
            .AnyAsync(d => d.EmployeeNumber.ToLower() == dto.EmployeeNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (empExists)
        {
            throw new InvalidOperationException($"Driver with employee number '{dto.EmployeeNumber}' already exists.");
        }

        var licExists = await _context.Drivers
            .AnyAsync(d => d.LicenseNumber.ToLower() == dto.LicenseNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (licExists)
        {
            throw new InvalidOperationException($"Driver with license number '{dto.LicenseNumber}' already exists.");
        }

        if (dto.LicenseExpiry <= dto.LicenseIssueDate)
        {
            throw new InvalidOperationException("License expiry date must be after license issue date.");
        }

        var driver = new Driver
        {
            UserId = dto.UserId,
            EmployeeNumber = dto.EmployeeNumber.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address.Trim(),
            LicenseNumber = dto.LicenseNumber.Trim().ToUpper(),
            LicenseClass = dto.LicenseClass.Trim(),
            LicenseIssueDate = dto.LicenseIssueDate,
            LicenseExpiry = dto.LicenseExpiry,
            Status = DriverStatus.Available,
            JoinDate = dto.JoinDate ?? DateTime.UtcNow,
            EmergencyContact = dto.EmergencyContact.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(driver);
    }

    public async Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverDto dto, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (driver == null) return null;

        var empExists = await _context.Drivers
            .AnyAsync(d => d.Id != id && d.EmployeeNumber.ToLower() == dto.EmployeeNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (empExists)
        {
            throw new InvalidOperationException($"Another driver with employee number '{dto.EmployeeNumber}' already exists.");
        }

        var licExists = await _context.Drivers
            .AnyAsync(d => d.Id != id && d.LicenseNumber.ToLower() == dto.LicenseNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (licExists)
        {
            throw new InvalidOperationException($"Another driver with license number '{dto.LicenseNumber}' already exists.");
        }

        if (dto.LicenseExpiry <= dto.LicenseIssueDate)
        {
            throw new InvalidOperationException("License expiry date must be after license issue date.");
        }

        driver.UserId = dto.UserId;
        driver.EmployeeNumber = dto.EmployeeNumber.Trim();
        driver.FirstName = dto.FirstName.Trim();
        driver.LastName = dto.LastName.Trim();
        driver.Phone = dto.Phone.Trim();
        driver.Email = dto.Email?.Trim();
        driver.Address = dto.Address.Trim();
        driver.LicenseNumber = dto.LicenseNumber.Trim().ToUpper();
        driver.LicenseClass = dto.LicenseClass.Trim();
        driver.LicenseIssueDate = dto.LicenseIssueDate;
        driver.LicenseExpiry = dto.LicenseExpiry;
        driver.Status = dto.Status;
        driver.EmergencyContact = dto.EmergencyContact.Trim();
        driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(driver);
    }

    public async Task<bool> DeactivateDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (driver == null) return false;

        driver.Status = DriverStatus.Inactive;
        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DriverDto MapToDto(Driver driver) => new()
    {
        Id = driver.Id,
        UserId = driver.UserId,
        EmployeeNumber = driver.EmployeeNumber,
        FirstName = driver.FirstName,
        LastName = driver.LastName,
        FullName = driver.FullName,
        Phone = driver.Phone,
        Email = driver.Email,
        Address = driver.Address,
        LicenseNumber = driver.LicenseNumber,
        LicenseClass = driver.LicenseClass,
        LicenseIssueDate = driver.LicenseIssueDate,
        LicenseExpiry = driver.LicenseExpiry,
        IsLicenseExpired = driver.IsLicenseExpired,
        Status = driver.Status,
        JoinDate = driver.JoinDate,
        EmergencyContact = driver.EmergencyContact,
        CreatedAt = driver.CreatedAt,
        UpdatedAt = driver.UpdatedAt
    };
}
