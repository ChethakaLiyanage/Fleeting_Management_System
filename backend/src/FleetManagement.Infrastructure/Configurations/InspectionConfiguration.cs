using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");

        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.Vehicle)
            .WithMany()
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Driver)
            .WithMany()
            .HasForeignKey(i => i.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Trip)
            .WithMany()
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Items)
            .WithOne(it => it.Inspection)
            .HasForeignKey(it => it.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasIndex(i => i.Result);
        builder.HasIndex(i => i.InspectionDate);
    }
}
