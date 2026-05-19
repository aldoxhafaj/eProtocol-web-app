using AutoMapper;
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
        var (number, year) = await protocolNumberService.NextAsync(cancellationToken);

        await using var fileStream = file.OpenReadStream();
        var storageResult = await fileStorage.SaveAsync(new FileStorageRequest(file.FileName, file.ContentType, fileStream, request.Classification == DocumentClassification.Secret), cancellationToken);

        var documentFile = new DocumentFile
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = storageResult.Size,
            Hash = storageResult.Hash,
            StoragePath = storageResult.StoragePath,
            IsEncrypted = request.Classification == DocumentClassification.Secret
        };

        var document = new Document
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Classification = request.Classification,
            Type = request.Type,
            Status = DocumentStatus.Registered,
            ProtocolNumber = number,
            ProtocolYear = year,
            InstitutionId = request.InstitutionId,
            Deadline = request.Deadline,
            CreatedById = userContext.UserId,
            File = documentFile
        };

        dbContext.DocumentFiles.Add(documentFile);
        dbContext.Documents.Add(document);
        dbContext.DocumentAudits.Add(new DocumentAudit
        {
            Document = document,
            Action = "Created",
            PerformedById = userContext.UserId
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<DocumentDto>(document);
    }

    public async Task AssignAsync(Guid documentId, AssignDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
        if (document is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        var assignment = new DocumentAssignment
        {
            DocumentId = documentId,
            UserId = request.UserId,
            AssignedById = userContext.UserId
        };

        dbContext.DocumentAssignments.Add(assignment);
        document.Status = DocumentStatus.Assigned;
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
            .Include(d => d.Assignments);

        query = ApplyAccessControl(query);
        var document = await query.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        return document is null ? null : mapper.Map<DocumentDto>(document);
    }

    public async Task<IReadOnlyList<DocumentDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Documents.AsNoTracking();

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

    private IQueryable<Document> ApplyAccessControl(IQueryable<Document> query)
    {
        if (string.Equals(userContext.Role, UserRole.Administrator.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userContext.Role, UserRole.Manager.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return query;
        }

        return query.Where(d => d.Classification == DocumentClassification.Public ||
                                (d.Classification == DocumentClassification.Restricted && d.Assignments.Any(a => a.UserId == userContext.UserId)));
    }
}
