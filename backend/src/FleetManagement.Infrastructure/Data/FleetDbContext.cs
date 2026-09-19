using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Infrastructure.Data;

public class FleetDbContext : DbContext
{
    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Entity configurations will be registered here
    }
}
