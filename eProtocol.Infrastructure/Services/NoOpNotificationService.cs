using eProtocol.Application.Abstractions;

namespace eProtocol.Infrastructure.Services;

public sealed class NoOpNotificationService : INotificationService
{
    public Task NotifyAssignmentAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyDeadlineAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
