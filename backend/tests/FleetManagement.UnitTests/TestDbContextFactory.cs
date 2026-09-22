using FleetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.UnitTests;

public static class TestDbContextFactory
{
    public static FleetDbContext Create()
    {
        var options = new DbContextOptionsBuilder<FleetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new FleetDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
