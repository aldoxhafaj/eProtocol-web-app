using eProtocol.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.AuthenticateAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}
