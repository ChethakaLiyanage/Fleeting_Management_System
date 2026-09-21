using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Interfaces;

public interface IFleetDbContext
{
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<VehicleAssignment> VehicleAssignments { get; }
    DbSet<Trip> Trips { get; }
    DbSet<Inspection> Inspections { get; }
    DbSet<InspectionItem> InspectionItems { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
