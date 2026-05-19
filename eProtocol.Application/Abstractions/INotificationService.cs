namespace eProtocol.Application.Abstractions;

public interface INotificationService
{
    Task NotifyAssignmentAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default);
    Task NotifyDeadlineAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default);
}
