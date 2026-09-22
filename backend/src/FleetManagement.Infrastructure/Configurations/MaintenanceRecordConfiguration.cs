using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.ServiceProvider)
            .HasMaxLength(200);

        builder.Property(m => m.Cost)
            .HasPrecision(12, 2);

        builder.Property(m => m.OdometerReading)
            .HasPrecision(12, 2);

        builder.Property(m => m.NextServiceOdometer)
            .HasPrecision(12, 2);

        builder.Ignore(m => m.IsOverdue);
        builder.Ignore(m => m.IsCompleted);

        builder.HasOne(m => m.Vehicle)
            .WithMany()
            .HasForeignKey(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
