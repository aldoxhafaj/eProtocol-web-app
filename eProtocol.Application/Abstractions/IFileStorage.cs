namespace eProtocol.Application.Abstractions;

public interface IFileStorage
{
    Task<FileStorageResult> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storagePath, bool isEncrypted, CancellationToken cancellationToken = default);
}

public record FileStorageRequest(string FileName, string ContentType, Stream Content, bool Encrypt);

public record FileStorageResult(Guid? ExistingFileId, string StoragePath, string Hash, long Size);
