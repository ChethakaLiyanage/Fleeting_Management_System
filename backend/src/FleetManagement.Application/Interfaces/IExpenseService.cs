using FleetManagement.Application.DTOs.Expenses;

namespace FleetManagement.Application.Interfaces;

public interface IExpenseService
{{
    Task<IEnumerable<ExpenseDto>> GetAllAsync();
    Task<IEnumerable<ExpenseDto>> GetByVehicleAsync(Guid vehicleId);
    Task<IEnumerable<ExpenseDto>> GetPendingAsync();
    Task<ExpenseDto> GetByIdAsync(Guid id);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request);
    Task<ExpenseDto> ApproveAsync(Guid id, ApproveExpenseRequest request);
    Task<ExpenseDto> RejectAsync(Guid id, RejectExpenseRequest request);
    Task DeleteAsync(Guid id);
}}
