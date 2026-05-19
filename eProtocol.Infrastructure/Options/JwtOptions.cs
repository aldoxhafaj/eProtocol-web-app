namespace eProtocol.Infrastructure.Options;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 480;
}
