using Microsoft.AspNetCore.Http;

namespace eProtocol.Application.Documents;

public interface IDocumentService
{
    Task<DocumentDto> CreateAsync(CreateDocumentRequest request, IFormFile file, CancellationToken cancellationToken = default);
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentRequest request, IFormFile? file, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentFileDownloadDto?> GetFileAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> SearchAsync(DocumentSearchRequest request, CancellationToken cancellationToken = default);
    Task AssignAsync(Guid documentId, AssignDocumentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAssignmentDto>?> GetAssignmentsAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<bool> RemoveAssignmentAsync(Guid documentId, Guid assignmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MyAssignmentDto>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
    Task CompleteAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task UnarchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
