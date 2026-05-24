using AutoMapper;
using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using eProtocol.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Users;

public class UserService(IApplicationDbContext dbContext, IPasswordHasher passwordHasher, IMapper mapper, IUserContext userContext) : IUserService
{
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userName = request.UserName.Trim();
        var email = request.Email.Trim();
        var normalizedUserName = userName.ToLowerInvariant();
        var normalizedEmail = email.ToLowerInvariant();

        var exists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.UserName.ToLower() == normalizedUserName || u.Email.ToLower() == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new ArgumentException("A user with the same username or email already exists.");
        }

        var user = new User
        {
            UserName = userName,
            FullName = request.FullName.Trim(),
            Email = email,
            Role = request.Role,
            Department = request.Department?.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            MustChangePassword = true
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return;
        }

        // Check if user has any document relations that would prevent deletion
        var hasCreatedDocuments = await dbContext.Documents.AnyAsync(d => d.CreatedById == id, cancellationToken);
        var hasAssignments = await dbContext.DocumentAssignments.AnyAsync(a => a.UserId == id || a.AssignedById == id, cancellationToken);
        var hasAudits = await dbContext.DocumentAudits.AnyAsync(a => a.PerformedById == id, cancellationToken);

        if (hasCreatedDocuments || hasAssignments || hasAudits)
        {
            throw new InvalidOperationException("Cannot delete user because they have related documents, assignments, or audit records.");
        }

        // Remove notifications for this user
        var notifications = await dbContext.Notifications
            .Where(n => n.UserId == id)
            .ToListAsync(cancellationToken);
        dbContext.Notifications.RemoveRange(notifications);

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);
        return users.Select(mapper.Map<UserDto>).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users.AsNoTracking()
            .Where(u => u.Role == role)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
        return users.Select(mapper.Map<UserDto>).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user is null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var userName = request.UserName.Trim();
        var email = request.Email.Trim();
        var normalizedUserName = userName.ToLowerInvariant();
        var normalizedEmail = email.ToLowerInvariant();

        var exists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id != id && (u.UserName.ToLower() == normalizedUserName || u.Email.ToLower() == normalizedEmail), cancellationToken);
        if (exists)
        {
            throw new ArgumentException("A user with the same username or email already exists.");
        }

        user.UserName = userName;
        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.Department = request.Department?.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserDto>(user);
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        user.PasswordHash = passwordHasher.Hash(newPassword);
        user.MustChangePassword = true;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<UserDto>> GetManagersAsync(CancellationToken cancellationToken = default)
    {
        return GetByRoleAsync(UserRole.Manager, cancellationToken);
    }

    public async Task<UserDto?> GetManagerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Manager, cancellationToken);

        return user is null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateManagerAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Manager, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var enforcedRequest = request with { Role = UserRole.Manager };
        return await UpdateAsync(id, enforcedRequest, cancellationToken);
    }

    public async Task<bool> DeleteManagerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Manager, cancellationToken);
        if (user is null)
        {
            return false;
        }

        // Remove notifications for this user
        var notifications = await dbContext.Notifications
            .Where(n => n.UserId == id)
            .ToListAsync(cancellationToken);
        dbContext.Notifications.RemoveRange(notifications);

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<UserDto>> GetEmployeesInScopeAsync(CancellationToken cancellationToken = default)
    {
        var manager = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        IQueryable<User> query = dbContext.Users.AsNoTracking().Where(u => u.Role == UserRole.Employee);

        if (manager is not null && !string.IsNullOrWhiteSpace(manager.Department))
        {
            query = query.Where(u => u.Department == manager.Department);
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync(cancellationToken);
        return users.Select(mapper.Map<UserDto>).ToList();
    }

    public async Task<UserDto?> GetEmployeeInScopeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var manager = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Employee, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (manager is not null && !string.IsNullOrWhiteSpace(manager.Department) && !string.Equals(user.Department, manager.Department, StringComparison.Ordinal))
        {
            return null;
        }

        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateEmployeeInScopeAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var manager = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Employee, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (manager is not null && !string.IsNullOrWhiteSpace(manager.Department) && !string.Equals(user.Department, manager.Department, StringComparison.Ordinal))
        {
            return null;
        }

        var enforcedRequest = request with { Role = UserRole.Employee };
        return await UpdateAsync(id, enforcedRequest, cancellationToken);
    }

    public async Task<bool> DeleteEmployeeInScopeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var manager = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Employee, cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (manager is not null && !string.IsNullOrWhiteSpace(manager.Department) && !string.Equals(user.Department, manager.Department, StringComparison.Ordinal))
        {
            return false;
        }

        // Remove assignments where this user is the assignee
        var assignments = await dbContext.DocumentAssignments
            .Where(a => a.UserId == id)
            .ToListAsync(cancellationToken);
        dbContext.DocumentAssignments.RemoveRange(assignments);

        // Remove assignments where this user assigned others
        var assignedByUser = await dbContext.DocumentAssignments
            .Where(a => a.AssignedById == id)
            .ToListAsync(cancellationToken);
        dbContext.DocumentAssignments.RemoveRange(assignedByUser);

        // Remove audits performed by this user
        var audits = await dbContext.DocumentAudits
            .Where(a => a.PerformedById == id)
            .ToListAsync(cancellationToken);
        dbContext.DocumentAudits.RemoveRange(audits);

        // Remove notifications for this user
        var notifications = await dbContext.Notifications
            .Where(n => n.UserId == id)
            .ToListAsync(cancellationToken);
        dbContext.Notifications.RemoveRange(notifications);

        // Delete documents created by this user (cascades handle their assignments/audits)
        var documents = await dbContext.Documents
            .Include(d => d.File)
            .Where(d => d.CreatedById == id)
            .ToListAsync(cancellationToken);

        foreach (var doc in documents)
        {
            // Clear notifications referencing these documents
            var docNotifications = await dbContext.Notifications
                .Where(n => n.DocumentId == doc.Id)
                .ToListAsync(cancellationToken);
            foreach (var n in docNotifications)
            {
                n.DocumentId = null;
            }
        }

        dbContext.Documents.RemoveRange(documents);

        // Remove orphaned files
        var fileIds = documents.Select(d => d.FileId).Distinct().ToList();
        foreach (var fileId in fileIds)
        {
            var otherRefs = await dbContext.Documents.CountAsync(d => d.FileId == fileId && !documents.Select(x => x.Id).Contains(d.Id), cancellationToken);
            if (otherRefs == 0)
            {
                var file = await dbContext.DocumentFiles.FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
                if (file is not null)
                {
                    dbContext.DocumentFiles.Remove(file);
                }
            }
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
