using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class FuelRecordConfiguration : IEntityTypeConfiguration<FuelRecord>
{
    public void Configure(EntityTypeBuilder<FuelRecord> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Litres)
            .HasPrecision(10, 2);

        builder.Property(f => f.CostPerLitre)
            .HasPrecision(10, 4);

        builder.Property(f => f.OdometerReading)
            .HasPrecision(12, 2);

        builder.Ignore(f => f.TotalCost);

        builder.HasOne(f => f.Vehicle)
            .WithMany()
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Driver)
            .WithMany()
            .HasForeignKey(f => f.DriverId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
