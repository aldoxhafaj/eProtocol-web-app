using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;

namespace eProtocol.Infrastructure.Services;

public sealed class NotificationStorageService(IApplicationDbContext dbContext) : INotificationService
{
    public async Task NotifyAssignmentAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            DocumentId = documentId,
            Message = "A new document has been delegated to you."
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyDeadlineAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            DocumentId = documentId,
            Message = "A delegation deadline has passed and your response is still pending."
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
