using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagement.Infrastructure.Configurations;

public class InspectionItemConfiguration : IEntityTypeConfiguration<InspectionItem>
{
    public void Configure(EntityTypeBuilder<InspectionItem> builder)
    {
        builder.ToTable("InspectionItems");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.ItemName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(it => it.Notes)
            .HasMaxLength(250);

        builder.HasIndex(it => it.Status);
    }
}
