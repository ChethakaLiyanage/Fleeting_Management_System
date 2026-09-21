using FleetManagement.Application.DTOs.Insurance;

namespace FleetManagement.Application.Interfaces;

public interface IInsuranceService
{{
    Task<IEnumerable<InsurancePolicyDto>> GetAllAsync();
    Task<IEnumerable<InsurancePolicyDto>> GetByVehicleAsync(Guid vehicleId);
    Task<IEnumerable<InsurancePolicyDto>> GetExpiringAsync();
    Task<InsurancePolicyDto> GetByIdAsync(Guid id);
    Task<InsurancePolicyDto> CreateAsync(CreateInsurancePolicyRequest request);
    Task DeleteAsync(Guid id);
}}
