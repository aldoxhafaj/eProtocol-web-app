using eProtocol.Application.Abstractions;

namespace eProtocol.Infrastructure.Services;

public sealed class StubScannerService : IScannerService
{
    public Task<Stream> ScanAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "No scanner hardware detected. Please configure a TWAIN/WIA compatible scanner device.");
    }
}
