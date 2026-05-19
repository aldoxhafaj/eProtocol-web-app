using eProtocol.Domain.Entities;

namespace eProtocol.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
