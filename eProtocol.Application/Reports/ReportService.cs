using AutoMapper;
using eProtocol.Application.Abstractions;
using eProtocol.Application.Documents;
using eProtocol.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Reports;

public class ReportService(IApplicationDbContext dbContext, IMapper mapper) : IReportService
{
    public async Task<IReadOnlyList<DocumentDto>> GetProtocolBookAsync(ProtocolBookRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Documents.AsNoTracking()
            .Include(d => d.Assignments).ThenInclude(a => a.User)
            .Where(d => d.CreatedAt >= request.From && d.CreatedAt <= request.To);

        if (request.Type.HasValue)
            query = query.Where(d => d.Type == request.Type.Value);
        if (request.Classification.HasValue)
            query = query.Where(d => d.Classification == request.Classification.Value);
        if (request.InstitutionId.HasValue)
            query = query.Where(d => d.InstitutionId == request.InstitutionId.Value);

        var docs = await query.OrderBy(d => d.ProtocolNumber).ToListAsync(cancellationToken);
        return docs.Select(mapper.Map<DocumentDto>).ToList();
    }

    public async Task<IReadOnlyList<OverdueAssignmentDto>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var overdue = await dbContext.DocumentAssignments
            .AsNoTracking()
            .Include(a => a.Document)
            .Include(a => a.User)
            .Where(a => !a.IsCompleted && a.Deadline.HasValue && a.Deadline.Value < now)
            .ToListAsync(cancellationToken);

        return overdue.Select(a => new OverdueAssignmentDto(
            a.Id,
            a.DocumentId,
            a.Document.Title,
            a.Document.ProtocolNumber,
            a.Document.ProtocolYear,
            a.UserId,
            a.User.UserName,
            a.Deadline!.Value,
            (int)(now - a.Deadline.Value).TotalDays
        )).ToList();
    }

    public async Task<GeneralStatisticsDto> GetStatisticsAsync(StatisticsRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Documents.AsNoTracking();
        if (request.From.HasValue)
            query = query.Where(d => d.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(d => d.CreatedAt <= request.To.Value);

        var docs = await query.Select(d => new { d.Type, d.Classification }).ToListAsync(cancellationToken);

        return new GeneralStatisticsDto(
            docs.Count(d => d.Type == DocumentType.IncomingExternal),
            docs.Count(d => d.Type == DocumentType.OutgoingExternal),
            docs.Count(d => d.Type == DocumentType.Internal),
            docs.Count(d => d.Classification == DocumentClassification.Public),
            docs.Count(d => d.Classification == DocumentClassification.Restricted),
            docs.Count(d => d.Classification == DocumentClassification.Secret));
    }

    public async Task<IReadOnlyList<DocumentDto>> GetByPriorityAsync(CancellationToken cancellationToken = default)
    {
        var docs = await dbContext.Documents.AsNoTracking()
            .Include(d => d.Assignments).ThenInclude(a => a.User)
            .OrderByDescending(d => d.Priority)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(mapper.Map<DocumentDto>).ToList();
    }
}
