namespace eProtocol.Application.Abstractions;

public interface IProtocolNumberService
{
    Task<(int Number, int Year)> NextAsync(CancellationToken cancellationToken = default);
}
