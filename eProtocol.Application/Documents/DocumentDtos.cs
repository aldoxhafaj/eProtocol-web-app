using eProtocol.Domain.Enums;

namespace eProtocol.Application.Documents;

public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    DocumentClassification Classification,
    DocumentType Type,
    DocumentStatus Status,
    DocumentPriority Priority,
    int ProtocolNumber,
    int ProtocolYear,
    Guid? InstitutionId,
    DateTimeOffset? Deadline,
    Guid FileId,
    IReadOnlyList<DocumentAssignmentDto>? Assignments);

public record DocumentAssignmentDto(
    Guid Id,
    Guid UserId,
    string? UserName,
    DateTimeOffset AssignedAt,
    DateTimeOffset? Deadline,
    bool IsCompleted,
    DateTimeOffset? CompletedAt);

public record CreateDocumentRequest(
    string Title,
    string? Description,
    DocumentClassification Classification,
    DocumentType Type,
    DocumentPriority Priority,
    Guid? InstitutionId,
    DateTimeOffset? Deadline);

public record AssignDocumentRequest(Guid UserId, DateTimeOffset? Deadline);

public record DocumentSearchRequest(
    DocumentType? Type,
    DocumentClassification? Classification,
    Guid? InstitutionId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20);
