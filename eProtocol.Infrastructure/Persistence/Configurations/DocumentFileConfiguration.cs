using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProtocol.Infrastructure.Persistence.Configurations;

public sealed class DocumentFileConfiguration : IEntityTypeConfiguration<DocumentFile>
{
    public void Configure(EntityTypeBuilder<DocumentFile> builder)
    {
        builder.Property(f => f.FileName).HasMaxLength(260).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Hash).HasMaxLength(128).IsRequired();
        builder.Property(f => f.StoragePath).HasMaxLength(500).IsRequired();
    }
}
