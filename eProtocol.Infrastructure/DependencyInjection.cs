using eProtocol.Application.Abstractions;
using eProtocol.Infrastructure.Options;
using eProtocol.Infrastructure.Persistence;
using eProtocol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eProtocol.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();
        services.AddScoped<IJwtTokenService, SimpleJwtTokenService>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IProtocolNumberService, ProtocolNumberService>();
        services.AddScoped<INotificationService, NotificationStorageService>();
        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddScoped<IScannerService, StubScannerService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
