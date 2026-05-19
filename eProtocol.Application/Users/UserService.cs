using AutoMapper;
using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Users;

public class UserService(IApplicationDbContext dbContext, IPasswordHasher passwordHasher, IMapper mapper) : IUserService
{
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role,
            PasswordHash = passwordHasher.Hash(request.Password)
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

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);
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

        user.UserName = request.UserName.Trim();
        user.Email = request.Email.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserDto>(user);
    }
}
