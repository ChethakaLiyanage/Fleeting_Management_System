using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TripNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.TripNumber)
            .IsUnique();

        builder.HasOne(t => t.Vehicle)
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Driver)
            .WithMany()
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.StartLocation)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Destination)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Purpose)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.StartingMileage)
            .HasPrecision(18, 2);

        builder.Property(t => t.EndingMileage)
            .HasPrecision(18, 2);

        builder.Property(t => t.Distance)
            .HasPrecision(18, 2);

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.StartTime);
        builder.HasIndex(t => new { t.VehicleId, t.StartTime });
        builder.HasIndex(t => new { t.DriverId, t.StartTime });
    }
}
