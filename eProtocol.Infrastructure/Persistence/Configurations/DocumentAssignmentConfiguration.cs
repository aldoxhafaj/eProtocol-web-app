using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProtocol.Infrastructure.Persistence.Configurations;

public sealed class DocumentAssignmentConfiguration : IEntityTypeConfiguration<DocumentAssignment>
{
    public void Configure(EntityTypeBuilder<DocumentAssignment> builder)
    {
        builder.HasOne(a => a.Document)
            .WithMany(d => d.Assignments)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Assignments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedBy)
            .WithMany()
            .HasForeignKey(a => a.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
