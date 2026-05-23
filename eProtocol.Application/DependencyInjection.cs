using System.Reflection;
using AutoMapper;
using eProtocol.Application.Auth;
using eProtocol.Application.Documents;
using eProtocol.Application.Institutions;
using eProtocol.Application.Notifications;
using eProtocol.Application.Reports;
using eProtocol.Application.Tracking;
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
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<IDocumentDeletionPolicy, DocumentDeletionPolicy>();

        return services;
    }
}
