using eProtocol.Application.Users;
using eProtocol.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
public sealed class AdminUsersController(IUserService userService) : ControllerBase
{
    [HttpGet("managers")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetManagers(CancellationToken cancellationToken)
    {
        var users = await userService.GetManagersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("managers/{id:guid}")]
    public async Task<ActionResult<UserDto>> GetManagerById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetManagerByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("managers")]
    public async Task<ActionResult<UserDto>> CreateManager([FromBody] CreateUserAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(
                new CreateUserRequest(request.UserName, request.FullName, request.Email, request.Password, UserRole.Manager, request.Department),
                cancellationToken);

            return CreatedAtAction(nameof(UsersController.GetById), "Users", new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("managers/{id:guid}")]
    public async Task<ActionResult<UserDto>> UpdateManager(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateManagerAsync(id, request, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("managers/{id:guid}")]
    public async Task<IActionResult> DeleteManager(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await userService.DeleteManagerAsync(id, cancellationToken);
        return deleted ? Ok() : NotFound();
    }
}
