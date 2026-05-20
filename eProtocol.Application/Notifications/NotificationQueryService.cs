using eProtocol.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Notifications;

public class NotificationQueryService(IApplicationDbContext dbContext, IUserContext userContext) : INotificationQueryService
{
    public async Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userContext.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications.Select(n => new NotificationDto(n.Id, n.Message, n.DocumentId, n.IsRead, n.CreatedAt)).ToList();
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Notifications
            .CountAsync(n => n.UserId == userContext.UserId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userContext.UserId, cancellationToken);
        if (notification is null) return;

        notification.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
