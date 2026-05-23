namespace eProtocol.Application.Documents;

public static class FileValidation
{
    public static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/tiff",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    public static bool IsValidContentType(string contentType) => AllowedContentTypes.Contains(contentType);

    public static bool ExceedsMaxSize(long length) => length > MaxFileSize;
}
