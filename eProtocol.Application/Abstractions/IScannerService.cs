namespace eProtocol.Application.Abstractions;

public interface IScannerService
{
    Task<Stream> ScanAsync(CancellationToken cancellationToken = default);
}
