using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        builder.Property(v => v.VIN)
            .HasMaxLength(100);

        builder.HasIndex(v => v.VIN)
            .IsUnique()
            .HasFilter("\"VIN\" IS NOT NULL");

        builder.Property(v => v.EngineNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.Property(v => v.Mileage)
            .HasPrecision(18, 2);

        builder.Property(v => v.PurchasePrice)
            .HasPrecision(18, 2);

        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.RegistrationExpiry);
    }
}
