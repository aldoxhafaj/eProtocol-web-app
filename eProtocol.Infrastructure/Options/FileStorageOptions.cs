namespace eProtocol.Infrastructure.Options;

public sealed class FileStorageOptions
{
    public string RootPath { get; init; } = "storage";
    public string EncryptionKey { get; init; } = string.Empty;
}
