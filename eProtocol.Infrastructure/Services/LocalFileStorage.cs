using System.Security.Cryptography;
using eProtocol.Application.Abstractions;
using eProtocol.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eProtocol.Infrastructure.Services;

public sealed class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly FileStorageOptions storageOptions = options.Value;

    public async Task<FileStorageResult> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(storageOptions.RootPath, AppContext.BaseDirectory);
        Directory.CreateDirectory(root);

        var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(request.FileName)}";
        var relativePath = Path.Combine("files", fileName);
        var fullPath = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var output = File.Create(fullPath);
        await request.Content.CopyToAsync(output, cancellationToken);
        await output.FlushAsync(cancellationToken);

        await using var hashStream = File.OpenRead(fullPath);
        var hash = await SHA256.HashDataAsync(hashStream, cancellationToken);
        var size = hashStream.Length;

        return new FileStorageResult(relativePath, Convert.ToHexString(hash), size);
    }

    public Task<Stream> OpenReadAsync(string storagePath, bool isEncrypted, CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(storageOptions.RootPath, AppContext.BaseDirectory);
        var fullPath = Path.Combine(root, storagePath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }
}
