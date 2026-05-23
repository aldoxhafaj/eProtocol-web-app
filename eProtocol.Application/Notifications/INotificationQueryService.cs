namespace eProtocol.Application.Notifications;

public interface INotificationQueryService
{
    Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
}

public record NotificationDto(Guid Id, string Message, Guid? DocumentId, bool IsRead, DateTimeOffset CreatedAt);
