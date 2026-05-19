namespace eProtocol.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default);
}
