using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Institution> Institutions { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentFile> DocumentFiles { get; }
    DbSet<DocumentAssignment> DocumentAssignments { get; }
    DbSet<DocumentAudit> DocumentAudits { get; }
    DbSet<ProtocolSequence> ProtocolSequences { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
