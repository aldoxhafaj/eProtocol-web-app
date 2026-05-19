using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class DocumentAudit : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public Guid PerformedById { get; set; }
    public User PerformedBy { get; set; } = null!;
    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
}
