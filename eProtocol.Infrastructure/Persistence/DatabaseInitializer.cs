using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using eProtocol.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eProtocol.Infrastructure.Persistence;

public sealed class DatabaseInitializer(ApplicationDbContext dbContext, IPasswordHasher passwordHasher) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var migrations = dbContext.Database.GetService<IMigrationsAssembly>();
        if (migrations.Migrations.Count > 0)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        var hasChanges = false;

        if (!await dbContext.Institutions.AnyAsync(cancellationToken))
        {
            dbContext.Institutions.Add(new Institution
            {
                Name = "Default Institution",
                ContactEmail = "contact@eprotocol.local",
                ContactPhone = "+00 000 000 000",
                Address = "Main Office",
                IsActive = true
            });
            hasChanges = true;
        }

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            dbContext.Users.Add(new User
            {
                UserName = "admin",
                Email = "admin@eprotocol.local",
                Role = UserRole.Administrator,
                PasswordHash = passwordHasher.Hash("Admin123!"),
                IsActive = true
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
