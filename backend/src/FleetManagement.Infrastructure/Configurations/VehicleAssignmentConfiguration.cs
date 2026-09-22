using FleetManagement.Domain.Entities;
using FleetManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class VehicleAssignmentConfiguration : IEntityTypeConfiguration<VehicleAssignment>
{
    public void Configure(EntityTypeBuilder<VehicleAssignment> builder)
    {
        builder.ToTable("VehicleAssignments");

        builder.HasKey(va => va.Id);

        builder.HasOne(va => va.Vehicle)
            .WithMany()
            .HasForeignKey(va => va.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(va => va.Driver)
            .WithMany()
            .HasForeignKey(va => va.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(va => va.Notes)
            .HasMaxLength(500);

        builder.HasIndex(va => va.Status);
        builder.HasIndex(va => va.AssignedAt);
    }
}
