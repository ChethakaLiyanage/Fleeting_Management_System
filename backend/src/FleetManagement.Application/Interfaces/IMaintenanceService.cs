using FleetManagement.Application.DTOs.Maintenance;

namespace FleetManagement.Application.Interfaces;

public interface IMaintenanceService
{
    Task<IEnumerable<MaintenanceRecordDto>> GetAllAsync();
    Task<IEnumerable<MaintenanceRecordDto>> GetByVehicleAsync(Guid vehicleId);
    Task<IEnumerable<MaintenanceRecordDto>> GetOverdueAsync();
    Task<MaintenanceRecordDto> GetByIdAsync(Guid id);
    Task<MaintenanceRecordDto> CreateAsync(CreateMaintenanceRequest request);
    Task<MaintenanceRecordDto> UpdateAsync(Guid id, UpdateMaintenanceRequest request);
    Task<MaintenanceRecordDto> UpdateStatusAsync(Guid id, UpdateMaintenanceStatusRequest request);
    Task DeleteAsync(Guid id);
}
