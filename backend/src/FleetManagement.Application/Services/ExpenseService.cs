using FleetManagement.Application.DTOs.Expenses;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class ExpenseService : IExpenseService
{{
    private readonly IFleetDbContext _context;

    public ExpenseService(IFleetDbContext context) => _context = context;

    public async Task<IEnumerable<ExpenseDto>> GetAllAsync()
    {{
        var expenses = await _context.Expenses
            .Include(e => e.Vehicle)
            .Include(e => e.Driver)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        return expenses.Select(MapToDto);
    }}

    public async Task<IEnumerable<ExpenseDto>> GetByVehicleAsync(Guid vehicleId)
    {{
        var expenses = await _context.Expenses
            .Include(e => e.Vehicle)
            .Include(e => e.Driver)
            .Where(e => e.VehicleId == vehicleId && !e.IsDeleted)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        return expenses.Select(MapToDto);
    }}

    public async Task<IEnumerable<ExpenseDto>> GetPendingAsync()
    {{
        var expenses = await _context.Expenses
            .Include(e => e.Vehicle)
            .Include(e => e.Driver)
            .Where(e => e.Status == ExpenseStatus.Pending && !e.IsDeleted)
            .OrderBy(e => e.ExpenseDate)
            .ToListAsync();

        return expenses.Select(MapToDto);
    }}

    public async Task<ExpenseDto> GetByIdAsync(Guid id)
    {{
        var expense = await _context.Expenses
            .Include(e => e.Vehicle)
            .Include(e => e.Driver)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new Exception($"Expense {{id}} not found.");

        return MapToDto(expense);
    }}

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request)
    {{
        var expense = new Expense
        {{
            VehicleId          = request.VehicleId,
            DriverId           = request.DriverId,
            SubmittedByUserId  = request.SubmittedByUserId,
            Category           = request.Category,
            Description        = request.Description,
            Amount             = request.Amount,
            ExpenseDate        = request.ExpenseDate,
            ReceiptReference   = request.ReceiptReference,
            Notes              = request.Notes,
            Status             = ExpenseStatus.Pending
        }};

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(expense.Id);
    }}

    public async Task<ExpenseDto> ApproveAsync(Guid id, ApproveExpenseRequest request)
    {{
        var expense = await _context.Expenses.FindAsync(id)
            ?? throw new Exception($"Expense {{id}} not found.");

        if (!expense.CanBeApproved)
            throw new InvalidOperationException($"Expense is already {{expense.Status}}.");

        expense.Status           = ExpenseStatus.Approved;
        expense.ApprovedByUserId = request.ApprovedByUserId;
        expense.ApprovedAt       = DateTime.UtcNow;
        expense.UpdatedAt        = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }}

    public async Task<ExpenseDto> RejectAsync(Guid id, RejectExpenseRequest request)
    {{
        var expense = await _context.Expenses.FindAsync(id)
            ?? throw new Exception($"Expense {{id}} not found.");

        if (!expense.CanBeRejected)
            throw new InvalidOperationException($"Expense is already {{expense.Status}}.");

        expense.Status           = ExpenseStatus.Rejected;
        expense.ApprovedByUserId = request.RejectedByUserId;
        expense.RejectionReason  = request.Reason;
        expense.UpdatedAt        = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }}

    public async Task DeleteAsync(Guid id)
    {{
        var expense = await _context.Expenses.FindAsync(id)
            ?? throw new Exception($"Expense {{id}} not found.");

        expense.IsDeleted = true;
        await _context.SaveChangesAsync();
    }}

    private static ExpenseDto MapToDto(Expense e) => new()
    {{
        Id                  = e.Id,
        VehicleId           = e.VehicleId,
        VehicleRegistration = e.Vehicle?.RegistrationNumber,
        DriverId            = e.DriverId,
        DriverName          = e.Driver != null ? $"{{e.Driver.FirstName}} {{e.Driver.LastName}}" : null,
        Category            = e.Category.ToString(),
        Status              = e.Status.ToString(),
        Description         = e.Description,
        Amount              = e.Amount,
        ExpenseDate         = e.ExpenseDate,
        ReceiptReference    = e.ReceiptReference,
        Notes               = e.Notes,
        ApprovedAt          = e.ApprovedAt,
        RejectionReason     = e.RejectionReason
    }};
}}
