using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProtocol.Infrastructure.Persistence.Configurations;

public sealed class AssignmentNoteConfiguration : IEntityTypeConfiguration<AssignmentNote>
{
    public void Configure(EntityTypeBuilder<AssignmentNote> builder)
    {
        builder.Property(n => n.Text).HasMaxLength(2000).IsRequired();

        builder.HasOne(n => n.Assignment)
            .WithMany(a => a.Notes)
            .HasForeignKey(n => n.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
