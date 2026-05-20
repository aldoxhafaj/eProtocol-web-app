using Microsoft.AspNetCore.Http;

namespace eProtocol.Application.Documents;

public interface IDocumentService
{
    Task<DocumentDto> CreateAsync(CreateDocumentRequest request, IFormFile file, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default);
    Task AssignAsync(Guid documentId, AssignDocumentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
    Task CompleteAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}
