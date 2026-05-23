using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class AssignmentNote : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public DocumentAssignment Assignment { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
}
