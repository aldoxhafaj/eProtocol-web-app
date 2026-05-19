using eProtocol.Application.Documents;

namespace eProtocol.Application.Reports;

public interface IReportService
{
    Task<IReadOnlyList<DocumentDto>> GetProtocolBookAsync(ProtocolBookRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> GetOverdueAsync(CancellationToken cancellationToken = default);
}
