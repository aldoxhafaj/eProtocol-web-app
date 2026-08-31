using eProtocol.Domain.Enums;

namespace eProtocol.Application.Abstractions;

public static class UserContextExtensions
{
    public static bool IsInRole(this IUserContext userContext, UserRole role) =>
        string.Equals(userContext.Role, role.ToString(), StringComparison.OrdinalIgnoreCase);

    public static bool IsAdmin(this IUserContext userContext) => userContext.IsInRole(UserRole.Admin);

    public static bool IsManager(this IUserContext userContext) => userContext.IsInRole(UserRole.Manager);

    public static bool IsAdminOrManager(this IUserContext userContext) =>
        userContext.IsAdmin() || userContext.IsManager();
}
