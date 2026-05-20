using eProtocol.Domain.Common;
using eProtocol.Domain.Enums;

namespace eProtocol.Domain.Entities;

public class Document : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentClassification Classification { get; set; } = DocumentClassification.Public;
    public DocumentType Type { get; set; } = DocumentType.Internal;
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public DocumentPriority Priority { get; set; } = DocumentPriority.Normal;
    public int ProtocolNumber { get; set; }
    public int ProtocolYear { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    public Guid FileId { get; set; }
    public DocumentFile File { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<DocumentAssignment> Assignments { get; set; } = new List<DocumentAssignment>();
    public ICollection<DocumentAudit> Audits { get; set; } = new List<DocumentAudit>();
}
