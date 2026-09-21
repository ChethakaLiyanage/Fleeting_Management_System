using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.PolicyNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(i => i.PolicyNumber)
            .IsUnique();

        builder.Property(i => i.Insurer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.PremiumAmount)
            .HasPrecision(12, 2);

        builder.Property(i => i.CoverageDetails)
            .HasMaxLength(1000);

        builder.Ignore(i => i.IsExpired);
        builder.Ignore(i => i.IsExpiringSoon);
        builder.Ignore(i => i.DaysUntilExpiry);

        builder.HasOne(i => i.Vehicle)
            .WithMany()
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
