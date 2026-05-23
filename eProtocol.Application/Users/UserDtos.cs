using eProtocol.Domain.Enums;

namespace eProtocol.Application.Users;

public record UserDto(Guid Id, string UserName, string FullName, string Email, string? Department, UserRole Role, bool IsActive, bool MustChangePassword);

public record CreateUserRequest(string UserName, string FullName, string Email, string Password, UserRole Role, string? Department = null);

public record UpdateUserRequest(string UserName, string FullName, string Email, UserRole Role, bool IsActive, string? Department = null);

public record CreateUserAccountRequest(string UserName, string FullName, string Email, string Password, string? Department = null);

public record ResetPasswordRequest(string NewPassword);
