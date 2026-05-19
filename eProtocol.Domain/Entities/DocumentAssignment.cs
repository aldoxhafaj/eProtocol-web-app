using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class DocumentAssignment : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid AssignedById { get; set; }
    public User AssignedBy { get; set; } = null!;
}
