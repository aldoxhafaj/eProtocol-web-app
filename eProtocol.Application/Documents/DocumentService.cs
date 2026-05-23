using AutoMapper;
using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using eProtocol.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Documents;

public class DocumentService(
    IApplicationDbContext dbContext,
    IFileStorage fileStorage,
    IProtocolNumberService protocolNumberService,
    IUserContext userContext,
    INotificationService notificationService,
    IMapper mapper) : IDocumentService
{
    public async Task<DocumentDto> CreateAsync(CreateDocumentRequest request, IFormFile file, CancellationToken cancellationToken = default)
    {
        var currentUserId = userContext.UserId;
        if (currentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Unable to identify the current user.");
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == currentUserId, cancellationToken);
        if (!userExists)
        {
            throw new UnauthorizedAccessException("The current user does not exist in the system.");
        }

        if (request.InstitutionId.HasValue)
        {
            var institutionExists = await dbContext.Institutions.AnyAsync(i => i.Id == request.InstitutionId.Value, cancellationToken);
            if (!institutionExists)
            {
                throw new InvalidOperationException("The specified institution does not exist.");
            }
        }

        var (number, year) = await protocolNumberService.NextAsync(cancellationToken);

        await using var fileStream = file.OpenReadStream();
        var storageResult = await fileStorage.SaveAsync(new FileStorageRequest(file.FileName, file.ContentType, fileStream, request.Classification == DocumentClassification.Secret), cancellationToken);

        var documentFile = await ResolveDocumentFileAsync(storageResult, file, request.Classification == DocumentClassification.Secret, cancellationToken);

        var document = new Document
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Classification = request.Classification,
            Type = request.Type,
            Priority = request.Priority,
            Status = DocumentStatus.Pending,
            ProtocolNumber = number,
            ProtocolYear = year,
            InstitutionId = request.InstitutionId,
            Deadline = request.Deadline,
            CreatedById = currentUserId,
            FileId = documentFile.Id
        };

        dbContext.Documents.Add(document);
        dbContext.DocumentAudits.Add(new DocumentAudit
        {
            Document = document,
            Action = "Created",
            PerformedById = currentUserId
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<DocumentDto>(document);
    }

    public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentRequest request, IFormFile? file, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .Include(d => d.File)
            .Include(d => d.Assignments)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (document is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            document.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            document.Description = request.Description.Trim();
        }

        if (request.Classification.HasValue)
        {
            document.Classification = request.Classification.Value;
        }

        if (request.Type.HasValue)
        {
            document.Type = request.Type.Value;
        }

        if (request.Priority.HasValue)
        {
            document.Priority = request.Priority.Value;
        }

        if (request.Deadline.HasValue)
        {
            document.Deadline = request.Deadline;
        }

        if (file is not null)
        {
            await using var fileStream = file.OpenReadStream();
            var shouldEncrypt = document.Classification == DocumentClassification.Secret;
            var storageResult = await fileStorage.SaveAsync(new FileStorageRequest(file.FileName, file.ContentType, fileStream, shouldEncrypt), cancellationToken);
            var documentFile = await ResolveDocumentFileAsync(storageResult, file, shouldEncrypt, cancellationToken);
            document.FileId = documentFile.Id;
        }

        document.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<DocumentDto>(document);
    }

    public async Task AssignAsync(Guid documentId, AssignDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, cancellationToken);
        if (document is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        if (document.Status == DocumentStatus.Archived)
        {
            throw new InvalidOperationException("Cannot assign tracking to an Archived document.");
        }

        var assignment = new DocumentAssignment
        {
            DocumentId = documentId,
            UserId = request.UserId,
            Deadline = request.Deadline,
            AssignedById = userContext.UserId,
            Status = Domain.Enums.AssignmentStatus.Pending
        };

        dbContext.DocumentAssignments.Add(assignment);

        // Automatically transition to InProgress
        if (document.Status == DocumentStatus.Pending)
        {
            document.Status = DocumentStatus.InProgress;
        }

        dbContext.DocumentAudits.Add(new DocumentAudit
        {
            DocumentId = documentId,
            Action = "Assigned",
            PerformedById = userContext.UserId,
            Notes = $"Assigned to {request.UserId}"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAssignmentAsync(request.UserId, documentId, cancellationToken);
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        IQueryable<Document> query = dbContext.Documents
            .AsNoTracking()
            .Include(d => d.Assignments)
            .Where(d => !d.IsDeleted);

        query = ApplyAccessControl(query);
        var document = await query.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        return document is null ? null : mapper.Map<DocumentDto>(document);
    }

    public async Task<DocumentFileDownloadDto?> GetFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .AsNoTracking()
            .Include(d => d.File)
            .Include(d => d.Assignments)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (document is null)
        {
            return null;
        }

        if (!CanAccessDocumentFile(document))
        {
            throw new UnauthorizedAccessException("You do not have access to this file.");
        }

        var stream = await fileStorage.OpenReadAsync(document.File.StoragePath, document.File.IsEncrypted, cancellationToken);
        return new DocumentFileDownloadDto(stream, document.File.ContentType, document.File.FileName);
    }

    public async Task<IReadOnlyList<DocumentDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Documents.AsNoTracking().Where(d => !d.IsDeleted);

        if (request.Type.HasValue)
        {
            query = query.Where(d => d.Type == request.Type.Value);
        }

        if (request.Classification.HasValue)
        {
            query = query.Where(d => d.Classification == request.Classification.Value);
        }

        if (request.InstitutionId.HasValue)
        {
            query = query.Where(d => d.InstitutionId == request.InstitutionId);
        }

        if (request.From.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= request.To.Value);
        }

        query = ApplyAccessControl(query);

        var skip = Math.Max(0, (request.Page - 1) * request.PageSize);
        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return documents.Select(mapper.Map<DocumentDto>).ToList();
    }

    public async Task<IReadOnlyList<DocumentAssignmentDto>?> GetAssignmentsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .AsNoTracking()
            .Include(d => d.Assignments)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, cancellationToken);

        if (document is null)
        {
            return null;
        }

        if (!CanAccessDocumentAssignments(document))
        {
            return null;
        }

        return document.Assignments.Select(mapper.Map<DocumentAssignmentDto>).ToList();
    }

    public async Task<bool> RemoveAssignmentAsync(Guid documentId, Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = await dbContext.DocumentAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.DocumentId == documentId, cancellationToken);

        if (assignment is null)
        {
            return false;
        }

        dbContext.DocumentAssignments.Remove(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<Document> ApplyAccessControl(IQueryable<Document> query)
    {
        if (IsAdminOrManager())
        {
            return query;
        }

        return query.Where(d => d.Classification == DocumentClassification.Public ||
                                (d.Classification == DocumentClassification.Restricted && d.Assignments.Any(a => a.UserId == userContext.UserId)));
    }

    public async Task<IReadOnlyList<DocumentDto>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        var documents = await dbContext.Documents
            .AsNoTracking()
            .Include(d => d.Assignments).ThenInclude(a => a.User)
            .Where(d => d.Assignments.Any(a => a.UserId == userContext.UserId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(mapper.Map<DocumentDto>).ToList();
    }

    public async Task CompleteAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = await dbContext.DocumentAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
        if (assignment is null)
            throw new InvalidOperationException("Assignment not found.");

        assignment.IsCompleted = true;
        assignment.CompletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .Include(d => d.File)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        if (document is null)
        {
            return false;
        }

        // Soft-delete by default (hard-delete via DeletionPolicy for admins)
        document.IsDeleted = true;
        document.DeletedAt = DateTimeOffset.UtcNow;
        document.DeletedById = userContext.UserId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        if (document is null)
            throw new InvalidOperationException("Document not found.");

        if (document.Status == DocumentStatus.Archived)
            throw new InvalidOperationException("Document is already archived.");

        document.Status = DocumentStatus.Archived;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.DocumentAudits.Add(new DocumentAudit
        {
            DocumentId = id,
            Action = "Archived",
            PerformedById = userContext.UserId
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents
            .Include(d => d.Assignments)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (document is null)
            throw new InvalidOperationException("Document not found.");

        if (document.Status != DocumentStatus.Archived)
            throw new InvalidOperationException("Document is not archived.");

        var hasIncompleteAssignments = document.Assignments.Any(a => !a.IsCompleted);
        document.Status = hasIncompleteAssignments ? DocumentStatus.InProgress : DocumentStatus.Pending;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.DocumentAudits.Add(new DocumentAudit
        {
            DocumentId = id,
            Action = "Unarchived",
            PerformedById = userContext.UserId
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<DocumentFile> ResolveDocumentFileAsync(FileStorageResult storageResult, IFormFile file, bool isEncrypted, CancellationToken cancellationToken)
    {
        if (storageResult.ExistingFileId.HasValue)
        {
            var existing = await dbContext.DocumentFiles.FirstOrDefaultAsync(f => f.Id == storageResult.ExistingFileId.Value, cancellationToken);
            if (existing is not null)
            {
                return existing;
            }
        }

        var documentFile = new DocumentFile
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = storageResult.Size,
            Hash = storageResult.Hash,
            StoragePath = storageResult.StoragePath,
            IsEncrypted = isEncrypted
        };

        dbContext.DocumentFiles.Add(documentFile);
        return documentFile;
    }

    private bool IsAdminOrManager()
    {
        return string.Equals(userContext.Role, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(userContext.Role, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(userContext.Role, UserRole.Manager.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private bool CanAccessDocumentFile(Document document)
    {
        if (IsAdminOrManager())
        {
            return true;
        }

        if (document.Classification == DocumentClassification.Secret)
        {
            return false;
        }

        return document.Assignments.Any(a => a.UserId == userContext.UserId);
    }

    private bool CanAccessDocumentAssignments(Document document)
    {
        if (IsAdminOrManager())
        {
            return true;
        }

        return document.Assignments.Any(a => a.UserId == userContext.UserId);
    }
}
