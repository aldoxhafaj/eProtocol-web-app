using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Documents;

internal static class DocumentRemoval
{
    /// <summary>
    /// Detaches notifications from the document and removes it, dropping its file when no
    /// other document still references it. Does not save changes.
    /// </summary>
    public static async Task RemoveWithOrphanFileAsync(
        IApplicationDbContext dbContext,
        Document document,
        CancellationToken cancellationToken)
    {
        var notifications = await dbContext.Notifications
            .Where(n => n.DocumentId == document.Id)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.DocumentId = null;
        }

        var otherReferences = await dbContext.Documents
            .CountAsync(d => d.FileId == document.FileId && d.Id != document.Id, cancellationToken);

        dbContext.Documents.Remove(document);
        if (otherReferences == 0)
        {
            dbContext.DocumentFiles.Remove(document.File);
        }
    }
}
