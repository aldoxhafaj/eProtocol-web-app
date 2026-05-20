using eProtocol.Application.Documents;

namespace eProtocol.Application.Reports;

public interface IReportService
{
    Task<IReadOnlyList<DocumentDto>> GetProtocolBookAsync(ProtocolBookRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OverdueAssignmentDto>> GetOverdueAsync(CancellationToken cancellationToken = default);
    Task<GeneralStatisticsDto> GetStatisticsAsync(StatisticsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> GetByPriorityAsync(CancellationToken cancellationToken = default);
}
