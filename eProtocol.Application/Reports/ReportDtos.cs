using eProtocol.Domain.Enums;

namespace eProtocol.Application.Reports;

public record ProtocolBookRequest(DateTimeOffset From, DateTimeOffset To, DocumentType? Type, DocumentClassification? Classification, Guid? InstitutionId);
