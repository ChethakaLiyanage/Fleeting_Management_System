using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FleetManagement.Application.Interfaces;

public interface IFleetDbContext
{
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Trip> Trips { get; }
    DbSet<Inspection> Inspections { get; }
    DbSet<InspectionItem> InspectionItems { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<FuelRecord> FuelRecords { get; }
    DbSet<MaintenanceRecord> MaintenanceRecords { get; }
    // DbSet<Expense> Expenses { get; }
    // DbSet<InsurancePolicy> InsurancePolicies { get; }
    // DbSet<Document> Documents { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
