using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Drivers;
using FleetManagement.Application.Exceptions;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Application.Services;

public class DriverService : IDriverService
{
    private readonly IFleetDbContext _context;
    private readonly IDashboardCache _dashboardCache;
    private readonly ILogger<DriverService> _logger;
    private readonly IPasswordService _passwordService;

    public DriverService(
        IFleetDbContext context,
        IDashboardCache dashboardCache,
        ILogger<DriverService> logger,
        IPasswordService passwordService)
    {
        _context        = context;
        _dashboardCache = dashboardCache;
        _logger         = logger;
        _passwordService = passwordService;
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
            .Select(d => new DriverDto
            {
                Id               = d.Id,
                DriverId         = d.Id,
                UserId           = d.UserId,
                EmployeeNumber   = d.EmployeeNumber,
                FirstName        = d.FirstName,
                LastName         = d.LastName,
                FullName         = d.FirstName + " " + d.LastName,
                Phone            = d.Phone,
                Email            = d.Email,
                Address          = d.Address,
                LicenseNumber    = d.LicenseNumber,
                LicenseClass     = d.LicenseClass,
                LicenseIssueDate = d.LicenseIssueDate,
                LicenseExpiry    = d.LicenseExpiry,
                IsLicenseExpired = d.LicenseExpiry <= DateTime.UtcNow,
                Status           = d.Status,
                JoinDate         = d.JoinDate,
                EmergencyContact = d.EmergencyContact,
                CreatedAt        = d.CreatedAt,
                UpdatedAt        = d.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DriverDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<DriverDto?> GetDriverByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsNoTracking()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Select(d => new DriverDto
            {
                Id               = d.Id,
                DriverId         = d.Id,
                UserId           = d.UserId,
                EmployeeNumber   = d.EmployeeNumber,
                FirstName        = d.FirstName,
                LastName         = d.LastName,
                FullName         = d.FirstName + " " + d.LastName,
                Phone            = d.Phone,
                Email            = d.Email,
                Address          = d.Address,
                LicenseNumber    = d.LicenseNumber,
                LicenseClass     = d.LicenseClass,
                LicenseIssueDate = d.LicenseIssueDate,
                LicenseExpiry    = d.LicenseExpiry,
                IsLicenseExpired = d.LicenseExpiry <= DateTime.UtcNow,
                Status           = d.Status,
                JoinDate         = d.JoinDate,
                EmergencyContact = d.EmergencyContact,
                CreatedAt        = d.CreatedAt,
                UpdatedAt        = d.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return driver;
    }

    public async Task<DriverHistoryDto?> GetDriverHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (driver == null) return null;

        return new DriverHistoryDto
        {
            DriverId                    = driver.Id,
            FullName                    = driver.FullName,
            LicenseNumber               = driver.LicenseNumber,
            Status                      = driver.Status,
            TotalTripsCompleted         = 0,
            TotalKilometersDriven       = 0,
            TotalIncidentsInvolved      = 0,
            CurrentlyAssignedVehicleId  = null,
            CurrentlyAssignedVehicleReg = null
        };
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken))
            throw new ConflictException($"A user with email '{dto.Email}' already exists.");

        var empExists = await _context.Drivers
            .AnyAsync(d => d.EmployeeNumber.ToLower() == dto.EmployeeNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (empExists)
        {
            throw new ConflictException($"Driver with employee number '{dto.EmployeeNumber}' already exists.");
        }

        var licExists = await _context.Drivers
            .AnyAsync(d => d.LicenseNumber.ToLower() == dto.LicenseNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (licExists)
        {
            throw new ConflictException($"Driver with license number '{dto.LicenseNumber}' already exists.");
        }

        if (dto.LicenseExpiry <= dto.LicenseIssueDate)
        {
            throw new InvalidOperationException("License expiry date must be after license issue date.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Driver", cancellationToken)
            ?? throw new InvalidOperationException("Driver role is not configured.");
        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordService.Hash(dto.InitialPassword),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { RoleId = role.Id });
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var driver = new Driver
        {
            UserId           = user.Id,
            EmployeeNumber   = dto.EmployeeNumber.Trim(),
            FirstName        = dto.FirstName.Trim(),
            LastName         = dto.LastName.Trim(),
            Phone            = dto.Phone.Trim(),
            Email            = email,
            Address          = dto.Address.Trim(),
            LicenseNumber    = dto.LicenseNumber.Trim().ToUpper(),
            LicenseClass     = dto.LicenseClass.Trim(),
            LicenseIssueDate = NormalizeUtc(dto.LicenseIssueDate),
            LicenseExpiry    = NormalizeUtc(dto.LicenseExpiry),
            Status           = DriverStatus.Available,
            JoinDate         = dto.JoinDate.HasValue ? NormalizeUtc(dto.JoinDate.Value) : DateTime.UtcNow,
            EmergencyContact = dto.EmergencyContact.Trim(),
            CreatedAt        = DateTime.UtcNow
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Driver created: {Id} {EmployeeNumber}", driver.Id, driver.EmployeeNumber);
        _dashboardCache.Invalidate();

        return MapToDto(driver);
    }

    public async Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverDto dto, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (driver == null) return null;

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Id != driver.UserId && u.Email.ToLower() == email, cancellationToken))
            throw new ConflictException($"A user with email '{dto.Email}' already exists.");

        var empExists = await _context.Drivers
            .AnyAsync(d => d.Id != id && d.EmployeeNumber.ToLower() == dto.EmployeeNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (empExists)
        {
            throw new ConflictException($"Another driver with employee number '{dto.EmployeeNumber}' already exists.");
        }

        var licExists = await _context.Drivers
            .AnyAsync(d => d.Id != id && d.LicenseNumber.ToLower() == dto.LicenseNumber.ToLower() && !d.IsDeleted, cancellationToken);
        if (licExists)
        {
            throw new ConflictException($"Another driver with license number '{dto.LicenseNumber}' already exists.");
        }

        if (dto.LicenseExpiry <= dto.LicenseIssueDate)
        {
            throw new InvalidOperationException("License expiry date must be after license issue date.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        driver.User.FirstName    = dto.FirstName.Trim();
        driver.User.LastName     = dto.LastName.Trim();
        driver.User.Email        = email;
        driver.EmployeeNumber   = dto.EmployeeNumber.Trim();
        driver.FirstName        = dto.FirstName.Trim();
        driver.LastName         = dto.LastName.Trim();
        driver.Phone            = dto.Phone.Trim();
        driver.Email            = email;
        driver.Address          = dto.Address.Trim();
        driver.LicenseNumber    = dto.LicenseNumber.Trim().ToUpper();
        driver.LicenseClass     = dto.LicenseClass.Trim();
        driver.LicenseIssueDate = NormalizeUtc(dto.LicenseIssueDate);
        driver.LicenseExpiry    = NormalizeUtc(dto.LicenseExpiry);
        driver.Status           = dto.Status;
        driver.EmergencyContact = dto.EmergencyContact.Trim();
        driver.UpdatedAt        = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Driver updated: {Id} {EmployeeNumber}", driver.Id, driver.EmployeeNumber);
        _dashboardCache.Invalidate();

        return MapToDto(driver);
    }

    private static DateTime NormalizeUtc(DateTime value) => value.Kind == DateTimeKind.Utc
        ? value
        : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public async Task<bool> DeactivateDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (driver == null) return false;

        if (await _context.Trips.AnyAsync(t => t.DriverId == id && !t.IsDeleted && t.Status != TripStatus.Completed && t.Status != TripStatus.Cancelled, cancellationToken))
            throw new InvalidOperationException("Cannot deactivate a driver with an active trip.");

        driver.Status    = DriverStatus.Inactive;
        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;

        var user = await _context.Users.FirstAsync(u => u.Id == driver.UserId, cancellationToken);
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Driver deactivated: {Id}", id);
        _dashboardCache.Invalidate();

        return true;
    }

    private static DriverDto MapToDto(Driver driver) => new()
    {
        Id               = driver.Id,
        DriverId         = driver.Id,
        UserId           = driver.UserId,
        EmployeeNumber   = driver.EmployeeNumber,
        FirstName        = driver.FirstName,
        LastName         = driver.LastName,
        FullName         = driver.FullName,
        Phone            = driver.Phone,
        Email            = driver.Email,
        Address          = driver.Address,
        LicenseNumber    = driver.LicenseNumber,
        LicenseClass     = driver.LicenseClass,
        LicenseIssueDate = driver.LicenseIssueDate,
        LicenseExpiry    = driver.LicenseExpiry,
        IsLicenseExpired = driver.IsLicenseExpired,
        Status           = driver.Status,
        JoinDate         = driver.JoinDate,
        EmergencyContact = driver.EmergencyContact,
        CreatedAt        = driver.CreatedAt,
        UpdatedAt        = driver.UpdatedAt
    };
}
