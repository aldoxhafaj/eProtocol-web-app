using eProtocol.Domain.Common;
using eProtocol.Domain.Enums;

namespace eProtocol.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Department { get; set; }
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public ICollection<DocumentAssignment> Assignments { get; set; } = new List<DocumentAssignment>();
    public ICollection<DocumentAudit> Audits { get; set; } = new List<DocumentAudit>();
}
