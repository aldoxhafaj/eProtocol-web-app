using eProtocol.Domain.Common;
using eProtocol.Domain.Enums;

namespace eProtocol.Domain.Entities;

public class DocumentAssignment : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Deadline { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid AssignedById { get; set; }
    public User AssignedBy { get; set; } = null!;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
    public string? CancelledReason { get; set; }
    public ICollection<AssignmentNote> Notes { get; set; } = new List<AssignmentNote>();
}
