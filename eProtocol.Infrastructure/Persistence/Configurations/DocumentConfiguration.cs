using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProtocol.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.Property(d => d.Title).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(2000);

        builder.HasOne(d => d.Institution)
            .WithMany(i => i.Documents)
            .HasForeignKey(d => d.InstitutionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.File)
            .WithMany()
            .HasForeignKey(d => d.FileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.CreatedBy)
            .WithMany()
            .HasForeignKey(d => d.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
