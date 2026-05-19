using eProtocol.Domain.Enums;

namespace eProtocol.Application.Documents;

public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    DocumentClassification Classification,
    DocumentType Type,
    DocumentStatus Status,
    int ProtocolNumber,
    int ProtocolYear,
    Guid? InstitutionId,
    DateTimeOffset? Deadline,
    Guid FileId);

public record CreateDocumentRequest(
    string Title,
    string? Description,
    DocumentClassification Classification,
    DocumentType Type,
    Guid? InstitutionId,
    DateTimeOffset? Deadline);

public record AssignDocumentRequest(Guid UserId);

public record DocumentSearchRequest(
    DocumentType? Type,
    DocumentClassification? Classification,
    Guid? InstitutionId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20);
