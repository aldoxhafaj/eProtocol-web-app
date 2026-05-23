using eProtocol.Domain.Enums;

namespace eProtocol.Application.Tracking;

public record TrackingAssignmentDto(
    Guid Id,
    Guid DocumentId,
    string DocumentTitle,
    Guid UserId,
    string UserName,
    DateTimeOffset AssignedAt,
    DateTimeOffset? Deadline,
    AssignmentStatus Status,
    bool IsCompleted,
    DateTimeOffset? CompletedAt,
    string? CancelledReason);

public record ReassignRequest(Guid NewUserId, string? Note);

public record UpdateDeadlineRequest(DateTimeOffset NewDeadline, string Reason);

public record AddNoteRequest(string Text);

public record CancelAssignmentRequest(string Reason);

public record UpdateStatusRequest(AssignmentStatus NewStatus);

public record AssignmentNoteDto(Guid Id, Guid AuthorId, string AuthorName, string Text, DateTimeOffset CreatedAt);
