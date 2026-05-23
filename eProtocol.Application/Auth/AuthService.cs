using eProtocol.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Auth;

public class AuthService(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUserContext userContext,
    ITokenBlacklist tokenBlacklist) : IAuthService
{
    public async Task<AuthResponse> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        var identifier = request.UserName.Trim().ToLower();
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.UserName.ToLower() == identifier || u.Email.ToLower() == identifier,
            cancellationToken);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = jwtTokenService.GenerateToken(user);
        return new AuthResponse(token, DateTimeOffset.UtcNow.AddHours(8));
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            tokenBlacklist.Blacklist(token);
        }

        return Task.CompletedTask;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("Current and new password are required.");
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new ArgumentException("User not found.");
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw new ArgumentException("Current password is invalid.");
        }

        if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
        {
            throw new ArgumentException("New password must be different from current password.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
