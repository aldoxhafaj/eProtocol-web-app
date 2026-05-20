using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? DocumentId { get; set; }
    public Document? Document { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
