namespace eProtocol.Application.Auth;

public record AuthRequest(string UserName, string Password);

public record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt);
