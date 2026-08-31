using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using eProtocol.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Documents;

public record DeletionResult(bool Allowed, int StatusCode, string? Reason);

public interface IDocumentDeletionPolicy
{
    Task<DeletionResult> EvaluateAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task ExecuteDeleteAsync(Guid documentId, CancellationToken cancellationToken = default);
}

public class DocumentDeletionPolicy(
    IApplicationDbContext dbContext,
    IUserContext userContext) : IDocumentDeletionPolicy
{
    public async Task<DeletionResult> EvaluateAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .Include(d => d.Assignments)
            .Include(d => d.File)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, cancellationToken);

        if (document is null)
            return new DeletionResult(false, 404, "Document not found.");

        // Admin can delete anything unconditionally
        if (userContext.IsAdmin())
            return new DeletionResult(true, 200, null);

        // Rule 4: Cannot delete a document with protocol number that is incoming/outgoing (official record)
        if (document.ProtocolNumber > 0 &&
            document.Type is DocumentType.IncomingExternal or DocumentType.OutgoingExternal &&
            document.Status != DocumentStatus.Pending)
        {
            return new DeletionResult(false, 409, "This document is part of the official protocol record and cannot be deleted.");
        }

        // Rule 1 & 2: Check tracking status restriction
        var hasActiveTracking = document.Assignments.Any(a =>
            a.Status is AssignmentStatus.InProgress or AssignmentStatus.Completed);

        if (hasActiveTracking)
            return new DeletionResult(false, 409, "Cannot delete a document with active or completed tracking assignments.");

        // Owner can delete their own
        if (document.CreatedById == userContext.UserId)
            return new DeletionResult(true, 200, null);

        // Manager can delete documents by employees in scope
        if (userContext.IsManager())
            return new DeletionResult(true, 200, null);

        return new DeletionResult(false, 403, "You do not have permission to delete this document.");
    }

    public async Task ExecuteDeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .Include(d => d.File)
            .Include(d => d.Assignments)
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document is null) return;

        // Hard delete - cascades will handle Assignments, Audits, and AssignmentNotes;
        // notification references are cleared to avoid FK violations.
        await DocumentRemoval.RemoveWithOrphanFileAsync(dbContext, document, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
