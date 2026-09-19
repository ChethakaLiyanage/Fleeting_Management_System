using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Interfaces;

public interface IFleetDbContext
{
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<VehicleAssignment> VehicleAssignments { get; }
    DbSet<Trip> Trips { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
