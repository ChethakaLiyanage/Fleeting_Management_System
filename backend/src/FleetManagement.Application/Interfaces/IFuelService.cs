using FleetManagement.Application.DTOs.Fuel;

namespace FleetManagement.Application.Interfaces;

public interface IFuelService
{
    Task<IEnumerable<FuelRecordDto>> GetAllAsync();
    Task<IEnumerable<FuelRecordDto>> GetByVehicleAsync(Guid vehicleId);
    Task<FuelRecordDto> GetByIdAsync(Guid id);
    Task<FuelRecordDto> CreateAsync(CreateFuelRecordRequest request);
    Task<FuelRecordDto> UpdateAsync(Guid id, UpdateFuelRecordRequest request);
    Task DeleteAsync(Guid id);
    Task<FuelEfficiencyDto> GetEfficiencyAsync(Guid vehicleId);
}
