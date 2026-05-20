using eProtocol.Application.Documents;
using eProtocol.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("protocol-book")]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetProtocolBook([FromQuery] ProtocolBookRequest request, CancellationToken cancellationToken)
    {
        var docs = await reportService.GetProtocolBookAsync(request, cancellationToken);
        return Ok(docs);
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<IReadOnlyList<OverdueAssignmentDto>>> GetOverdue(CancellationToken cancellationToken)
    {
        var result = await reportService.GetOverdueAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<GeneralStatisticsDto>> GetStatistics([FromQuery] StatisticsRequest request, CancellationToken cancellationToken)
    {
        var result = await reportService.GetStatisticsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-priority")]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetByPriority(CancellationToken cancellationToken)
    {
        var result = await reportService.GetByPriorityAsync(cancellationToken);
        return Ok(result);
    }
}
