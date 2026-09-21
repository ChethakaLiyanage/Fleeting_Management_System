using FleetManagement.Application.DTOs.Insurance;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class InsuranceService : IInsuranceService
{{
    private readonly IFleetDbContext _context;

    public InsuranceService(IFleetDbContext context) => _context = context;

    public async Task<IEnumerable<InsurancePolicyDto>> GetAllAsync()
    {{
        var policies = await _context.InsurancePolicies
            .Include(p => p.Vehicle)
            .OrderBy(p => p.ExpiryDate)
            .ToListAsync();

        return policies.Select(MapToDto);
    }}

    public async Task<IEnumerable<InsurancePolicyDto>> GetByVehicleAsync(Guid vehicleId)
    {{
        var policies = await _context.InsurancePolicies
            .Include(p => p.Vehicle)
            .Where(p => p.VehicleId == vehicleId && !p.IsDeleted)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        return policies.Select(MapToDto);
    }}

    public async Task<IEnumerable<InsurancePolicyDto>> GetExpiringAsync()
    {{
        var threshold = DateTime.UtcNow.AddDays(30);
        var policies = await _context.InsurancePolicies
            .Include(p => p.Vehicle)
            .Where(p => !p.IsDeleted && p.ExpiryDate <= threshold && p.ExpiryDate >= DateTime.UtcNow)
            .OrderBy(p => p.ExpiryDate)
            .ToListAsync();

        return policies.Select(MapToDto);
    }}

    public async Task<InsurancePolicyDto> GetByIdAsync(Guid id)
    {{
        var policy = await _context.InsurancePolicies
            .Include(p => p.Vehicle)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new Exception($"Insurance policy {{id}} not found.");

        return MapToDto(policy);
    }}

    public async Task<InsurancePolicyDto> CreateAsync(CreateInsurancePolicyRequest request)
    {{
        var policy = new InsurancePolicy
        {{
            VehicleId       = request.VehicleId,
            PolicyType      = request.PolicyType,
            PolicyNumber    = request.PolicyNumber,
            Insurer         = request.Insurer,
            PremiumAmount   = request.PremiumAmount,
            StartDate       = request.StartDate,
            ExpiryDate      = request.ExpiryDate,
            CoverageDetails = request.CoverageDetails,
            Notes           = request.Notes
        }};

        _context.InsurancePolicies.Add(policy);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(policy.Id);
    }}

    public async Task DeleteAsync(Guid id)
    {{
        var policy = await _context.InsurancePolicies.FindAsync(id)
            ?? throw new Exception($"Insurance policy {{id}} not found.");

        policy.IsDeleted = true;
        await _context.SaveChangesAsync();
    }}

    private static InsurancePolicyDto MapToDto(InsurancePolicy p) => new()
    {{
        Id                  = p.Id,
        VehicleId           = p.VehicleId,
        VehicleRegistration = p.Vehicle?.RegistrationNumber,
        PolicyType          = p.PolicyType.ToString(),
        PolicyNumber        = p.PolicyNumber,
        Insurer             = p.Insurer,
        PremiumAmount       = p.PremiumAmount,
        StartDate           = p.StartDate,
        ExpiryDate          = p.ExpiryDate,
        CoverageDetails     = p.CoverageDetails,
        Notes               = p.Notes,
        IsExpired           = p.IsExpired,
        IsExpiringSoon      = p.IsExpiringSoon,
        DaysUntilExpiry     = p.DaysUntilExpiry
    }};
}}
