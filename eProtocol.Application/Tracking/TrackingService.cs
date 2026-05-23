using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using eProtocol.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Tracking;

public class TrackingService(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    INotificationService notificationService) : ITrackingService
{
    public async Task<TrackingAssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var a = await dbContext.DocumentAssignments
            .AsNoTracking()
            .Include(x => x.Document)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (a is null) return null;

        return new TrackingAssignmentDto(
            a.Id, a.DocumentId, a.Document.Title, a.UserId, a.User.UserName,
            a.AssignedAt, a.Deadline, a.Status, a.IsCompleted, a.CompletedAt, a.CancelledReason);
    }

    public async Task ReassignAsync(Guid id, ReassignRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrow(id, cancellationToken);
        EnsureNotTerminal(assignment);

        var previousUserId = assignment.UserId;
        assignment.UserId = request.NewUserId;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            dbContext.AssignmentNotes.Add(new AssignmentNote
            {
                AssignmentId = id,
                AuthorId = userContext.UserId,
                Text = request.Note
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Notify previous assignee of removal
        await notificationService.NotifyAssignmentAsync(previousUserId, assignment.DocumentId, cancellationToken);
        // Notify new assignee
        await notificationService.NotifyAssignmentAsync(request.NewUserId, assignment.DocumentId, cancellationToken);
    }

    public async Task UpdateDeadlineAsync(Guid id, UpdateDeadlineRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrow(id, cancellationToken);
        EnsureNotTerminal(assignment);

        assignment.Deadline = request.NewDeadline;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.AssignmentNotes.Add(new AssignmentNote
        {
            AssignmentId = id,
            AuthorId = userContext.UserId,
            Text = $"Deadline updated to {request.NewDeadline:O}. Reason: {request.Reason}"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssignmentNoteDto> AddNoteAsync(Guid id, AddNoteRequest request, CancellationToken cancellationToken = default)
    {
        _ = await GetAssignmentOrThrow(id, cancellationToken);

        var note = new AssignmentNote
        {
            AssignmentId = id,
            AuthorId = userContext.UserId,
            Text = request.Text.Trim()
        };
        dbContext.AssignmentNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        var author = await dbContext.Users.AsNoTracking().FirstAsync(u => u.Id == userContext.UserId, cancellationToken);
        return new AssignmentNoteDto(note.Id, note.AuthorId, author.UserName, note.Text, note.CreatedAt);
    }

    public async Task CancelAsync(Guid id, CancelAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrow(id, cancellationToken);
        EnsureNotTerminal(assignment);

        assignment.Status = AssignmentStatus.Cancelled;
        assignment.CancelledReason = request.Reason;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAssignmentAsync(assignment.UserId, assignment.DocumentId, cancellationToken);

        // Check if all assignments on the document are terminal — update document status
        await UpdateDocumentStatusAfterAssignmentChange(assignment.DocumentId, cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrow(id, cancellationToken);
        ValidateTransition(assignment.Status, request.NewStatus);

        assignment.Status = request.NewStatus;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.NewStatus == AssignmentStatus.Completed)
        {
            assignment.IsCompleted = true;
            assignment.CompletedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await UpdateDocumentStatusAfterAssignmentChange(assignment.DocumentId, cancellationToken);
    }

    private async Task<DocumentAssignment> GetAssignmentOrThrow(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.DocumentAssignments
            .Include(a => a.Document)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
            throw new InvalidOperationException("Tracking assignment not found.");

        return assignment;
    }

    private static void EnsureNotTerminal(DocumentAssignment assignment)
    {
        if (assignment.Status is AssignmentStatus.Completed or AssignmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot modify a tracking record in '{assignment.Status}' status. Terminal states do not allow further transitions.");
    }

    private static void ValidateTransition(AssignmentStatus current, AssignmentStatus target)
    {
        var valid = current switch
        {
            AssignmentStatus.Pending => target is AssignmentStatus.InProgress or AssignmentStatus.Cancelled,
            AssignmentStatus.InProgress => target is AssignmentStatus.Completed or AssignmentStatus.Cancelled,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException(
                $"Invalid status transition from '{current}' to '{target}'. " +
                $"Valid transitions: {GetValidTransitions(current)}.");
    }

    private static string GetValidTransitions(AssignmentStatus current) => current switch
    {
        AssignmentStatus.Pending => "InProgress, Cancelled",
        AssignmentStatus.InProgress => "Completed, Cancelled",
        _ => "None (terminal state)"
    };

    private async Task UpdateDocumentStatusAfterAssignmentChange(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents
            .Include(d => d.Assignments)
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document is null || document.Status == DocumentStatus.Archived) return;

        var activeAssignments = document.Assignments
            .Where(a => a.Status != AssignmentStatus.Cancelled)
            .ToList();

        if (activeAssignments.Count == 0) return;

        var allTerminal = activeAssignments.All(a => a.Status is AssignmentStatus.Completed or AssignmentStatus.Cancelled);
        var anyCompleted = activeAssignments.Any(a => a.Status == AssignmentStatus.Completed);

        if (allTerminal && anyCompleted)
        {
            document.Status = DocumentStatus.Completed;
        }
        else if (activeAssignments.Any(a => a.Status is AssignmentStatus.Pending or AssignmentStatus.InProgress))
        {
            document.Status = DocumentStatus.UnderReview;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
