namespace eProtocol.Infrastructure.Persistence;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
