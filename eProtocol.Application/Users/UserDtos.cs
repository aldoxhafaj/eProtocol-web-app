using eProtocol.Domain.Enums;

namespace eProtocol.Application.Users;

public record UserDto(Guid Id, string UserName, string Email, UserRole Role, bool IsActive);

public record CreateUserRequest(string UserName, string Email, string Password, UserRole Role);

public record UpdateUserRequest(string UserName, string Email, UserRole Role, bool IsActive);
