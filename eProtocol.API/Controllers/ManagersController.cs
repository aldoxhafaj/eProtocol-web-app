using eProtocol.Application.Users;
using eProtocol.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/managers")]
[Authorize(Roles = "Manager")]
public sealed class ManagersController(IUserService userService) : ControllerBase
{
    [HttpGet("employees")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetEmployees(CancellationToken cancellationToken)
    {
        var users = await userService.GetEmployeesInScopeAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("employees/{id:guid}")]
    public async Task<ActionResult<UserDto>> GetEmployeeById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetEmployeeInScopeByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("employees")]
    public async Task<ActionResult<UserDto>> CreateEmployee([FromBody] CreateUserAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(
                new CreateUserRequest(request.UserName, request.FullName, request.Email, request.Password, UserRole.Employee, request.Department),
                cancellationToken);

            return CreatedAtAction(nameof(UsersController.GetById), "Users", new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("employees/{id:guid}")]
    public async Task<ActionResult<UserDto>> UpdateEmployee(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateEmployeeInScopeAsync(id, request, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("employees/{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await userService.DeleteEmployeeInScopeAsync(id, cancellationToken);
        return deleted ? Ok() : NotFound();
    }
}
