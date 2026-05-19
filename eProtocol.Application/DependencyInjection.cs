using System.Reflection;
using AutoMapper;
using eProtocol.Application.Auth;
using eProtocol.Application.Documents;
using eProtocol.Application.Institutions;
using eProtocol.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace eProtocol.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}
