using eProtocol.Domain.Enums;

namespace eProtocol.Application.Users;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetManagersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetManagerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateManagerAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteManagerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetEmployeesInScopeAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetEmployeeInScopeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateEmployeeInScopeAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEmployeeInScopeAsync(Guid id, CancellationToken cancellationToken = default);
}
