using Microsoft.AspNetCore.Http;

namespace eProtocol.Application.Documents;

public static class FileValidationMessages
{
    public const string FileRequired = "File is required.";
    public const string InvalidContentType = "File type is not allowed.";

    public static string MaxSizeExceeded => $"File size exceeds maximum of {FileValidation.MaxFileSize / (1024 * 1024)} MB.";

    /// <summary>
    /// Returns the validation error for a required uploaded file, or null when the file is acceptable.
    /// </summary>
    public static string? ValidateRequired(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return FileRequired;
        }

        if (FileValidation.ExceedsMaxSize(file.Length))
        {
            return MaxSizeExceeded;
        }

        return FileValidation.IsValidContentType(file.ContentType) ? null : InvalidContentType;
    }
}
