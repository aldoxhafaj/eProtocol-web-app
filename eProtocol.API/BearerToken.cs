using Microsoft.Extensions.Primitives;

namespace eProtocol.API;

public static class BearerToken
{
    private const string Prefix = "Bearer ";

    /// <summary>
    /// Extracts the raw token from an Authorization header value, tolerating a missing scheme prefix.
    /// </summary>
    public static string Extract(StringValues authorizationHeader)
    {
        var value = authorizationHeader.ToString();
        return value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            ? value[Prefix.Length..].Trim()
            : value.Trim();
    }
}
