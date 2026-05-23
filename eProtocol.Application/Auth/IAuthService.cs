namespace eProtocol.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(string token, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
