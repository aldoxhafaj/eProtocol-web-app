namespace eProtocol.Application.Tracking;

public interface ITrackingService
{
    Task<TrackingAssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReassignAsync(Guid id, ReassignRequest request, CancellationToken cancellationToken = default);
    Task UpdateDeadlineAsync(Guid id, UpdateDeadlineRequest request, CancellationToken cancellationToken = default);
    Task<AssignmentNoteDto> AddNoteAsync(Guid id, AddNoteRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid id, CancelAssignmentRequest request, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, UpdateStatusRequest request, CancellationToken cancellationToken = default);
}
