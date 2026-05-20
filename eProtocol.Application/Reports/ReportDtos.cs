using eProtocol.Domain.Enums;

namespace eProtocol.Application.Reports;

public record ProtocolBookRequest(DateTimeOffset From, DateTimeOffset To, DocumentType? Type, DocumentClassification? Classification, Guid? InstitutionId);

public record StatisticsRequest(DateTimeOffset? From, DateTimeOffset? To);

public record GeneralStatisticsDto(
    int TotalIncoming,
    int TotalOutgoing,
    int TotalInternal,
    int TotalPublic,
    int TotalRestricted,
    int TotalSecret);

public record OverdueAssignmentDto(
    Guid AssignmentId,
    Guid DocumentId,
    string DocumentTitle,
    int ProtocolNumber,
    int ProtocolYear,
    Guid UserId,
    string UserName,
    DateTimeOffset Deadline,
    int DaysOverdue);
