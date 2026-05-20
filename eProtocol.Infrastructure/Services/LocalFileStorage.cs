using System.Security.Cryptography;
using eProtocol.Application.Abstractions;
using eProtocol.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eProtocol.Infrastructure.Services;

public sealed class LocalFileStorage(IOptions<FileStorageOptions> options, IApplicationDbContext dbContext) : IFileStorage
{
    private readonly FileStorageOptions storageOptions = options.Value;

    public async Task<FileStorageResult> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(storageOptions.RootPath, AppContext.BaseDirectory);
        Directory.CreateDirectory(root);

        // Read content into memory to compute hash first
        using var ms = new MemoryStream();
        await request.Content.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        var hashBytes = await SHA256.HashDataAsync(ms, cancellationToken);
        var hash = Convert.ToHexString(hashBytes);
        ms.Position = 0;

        // Deduplication: check if file with same hash already exists
        var existing = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(dbContext.DocumentFiles, f => f.Hash == hash, cancellationToken);

        if (existing is not null)
        {
            return new FileStorageResult(existing.StoragePath, hash, ms.Length);
        }

        var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(request.FileName)}";
        var relativePath = Path.Combine("files", fileName);
        var fullPath = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        if (request.Encrypt)
        {
            await EncryptAndWriteAsync(ms, fullPath, cancellationToken);
        }
        else
        {
            await using var output = File.Create(fullPath);
            await ms.CopyToAsync(output, cancellationToken);
        }

        return new FileStorageResult(relativePath, hash, ms.Length);
    }

    public Task<Stream> OpenReadAsync(string storagePath, bool isEncrypted, CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(storageOptions.RootPath, AppContext.BaseDirectory);
        var fullPath = Path.Combine(root, storagePath);

        if (isEncrypted)
        {
            return DecryptAndReadAsync(fullPath, cancellationToken);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    private async Task EncryptAndWriteAsync(Stream input, string outputPath, CancellationToken cancellationToken)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(storageOptions.EncryptionKey);
        aes.GenerateIV();

        await using var output = File.Create(outputPath);
        // Write IV first
        await output.WriteAsync(aes.IV, cancellationToken);

        await using var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await input.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
    }

    private async Task<Stream> DecryptAndReadAsync(string inputPath, CancellationToken cancellationToken)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(storageOptions.EncryptionKey);

        await using var input = File.OpenRead(inputPath);
        var iv = new byte[16];
        await input.ReadExactlyAsync(iv, cancellationToken);
        aes.IV = iv;

        var ms = new MemoryStream();
        await using var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        await cryptoStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;
        return ms;
    }
}
