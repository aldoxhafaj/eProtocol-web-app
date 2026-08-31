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
            .Where(d => !d.IsDeleted)
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
        var query = dbContext.Documents.AsNoTracking().Where(d => !d.IsDeleted);
        if (request.From.HasValue)
            query = query.Where(d => d.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(d => d.CreatedAt <= request.To.Value);

        var docs = await query.Select(d => new { d.Type, d.Classification }).ToListAsync(cancellationToken);

        int incoming = 0, outgoing = 0, internalCount = 0, publicCount = 0, restricted = 0, secret = 0;
        foreach (var doc in docs)
        {
            switch (doc.Type)
            {
                case DocumentType.IncomingExternal: incoming++; break;
                case DocumentType.OutgoingExternal: outgoing++; break;
                case DocumentType.Internal: internalCount++; break;
            }

            switch (doc.Classification)
            {
                case DocumentClassification.Public: publicCount++; break;
                case DocumentClassification.Restricted: restricted++; break;
                case DocumentClassification.Secret: secret++; break;
            }
        }

        return new GeneralStatisticsDto(incoming, outgoing, internalCount, publicCount, restricted, secret);
    }

    public async Task<IReadOnlyList<DocumentDto>> GetByPriorityAsync(CancellationToken cancellationToken = default)
    {
        var docs = await dbContext.Documents.AsNoTracking()
            .Include(d => d.Assignments).ThenInclude(a => a.User)
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.Priority)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(mapper.Map<DocumentDto>).ToList();
    }
}
