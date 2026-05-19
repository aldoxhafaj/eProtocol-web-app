using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProtocol.Infrastructure.Persistence.Configurations;

public sealed class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ContactEmail).HasMaxLength(320);
        builder.Property(i => i.ContactPhone).HasMaxLength(50);
        builder.Property(i => i.Address).HasMaxLength(500);
        builder.HasIndex(i => i.Name).IsUnique();
    }
}
