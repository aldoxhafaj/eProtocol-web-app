using System.Security.Claims;
using eProtocol.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace eProtocol.Infrastructure.Services;

public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public string Role
    {
        get
        {
            var role = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
                ?? httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;
            return string.IsNullOrWhiteSpace(role) ? "Employee" : role;
        }
    }
}
