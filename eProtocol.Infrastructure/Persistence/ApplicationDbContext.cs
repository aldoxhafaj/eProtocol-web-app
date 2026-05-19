using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentFile> DocumentFiles => Set<DocumentFile>();
    public DbSet<DocumentAssignment> DocumentAssignments => Set<DocumentAssignment>();
    public DbSet<DocumentAudit> DocumentAudits => Set<DocumentAudit>();
    public DbSet<ProtocolSequence> ProtocolSequences => Set<ProtocolSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
