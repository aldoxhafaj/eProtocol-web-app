using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Auth;

public class AuthService(IApplicationDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponse> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = jwtTokenService.GenerateToken(user);
        return new AuthResponse(token, DateTimeOffset.UtcNow.AddHours(8));
    }
}
