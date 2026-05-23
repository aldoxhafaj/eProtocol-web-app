using System.Collections.Concurrent;
using eProtocol.Application.Abstractions;

namespace eProtocol.Infrastructure.Services;

public sealed class InMemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, byte> blacklistedTokens = new(StringComparer.Ordinal);

    public void Blacklist(string token)
    {
        blacklistedTokens.TryAdd(token, 0);
    }

    public bool IsBlacklisted(string token)
    {
        return blacklistedTokens.ContainsKey(token);
    }
}
