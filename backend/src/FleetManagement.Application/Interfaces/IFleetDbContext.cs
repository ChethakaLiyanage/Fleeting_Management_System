using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Interfaces;

public interface IFleetDbContext
{
    DbSet<Vehicle> Vehicles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
