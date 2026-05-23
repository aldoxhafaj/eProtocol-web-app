using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProtocol.Infrastructure.Persistence.Configurations;

public sealed class DocumentAuditConfiguration : IEntityTypeConfiguration<DocumentAudit>
{
    public void Configure(EntityTypeBuilder<DocumentAudit> builder)
    {
        builder.HasOne(a => a.Document)
            .WithMany(d => d.Audits)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PerformedBy)
            .WithMany(u => u.Audits)
            .HasForeignKey(a => a.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
