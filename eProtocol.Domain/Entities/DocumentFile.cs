using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class DocumentFile : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
}
