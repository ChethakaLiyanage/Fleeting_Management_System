using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId).IsRequired();
        builder.HasIndex(d => d.UserId).IsUnique();

        builder.Property(d => d.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(d => d.EmployeeNumber)
            .IsUnique();

        builder.Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(d => d.Email)
            .HasMaxLength(150);

        builder.Property(d => d.Address)
            .HasMaxLength(250);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique();

        builder.Property(d => d.LicenseClass)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.EmergencyContact)
            .HasMaxLength(100);

        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.LicenseExpiry);
    }
}
