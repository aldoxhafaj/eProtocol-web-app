using eProtocol.Application.Abstractions;
using eProtocol.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Infrastructure.Services;

public sealed class ProtocolNumberService(ApplicationDbContext dbContext) : IProtocolNumberService
{
    public async Task<(int Number, int Year)> NextAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTimeOffset.UtcNow.Year;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var sequence = await dbContext.ProtocolSequences.FirstOrDefaultAsync(x => x.Year == year, cancellationToken);

        if (sequence is null)
        {
            sequence = new Domain.Entities.ProtocolSequence { Year = year, LastNumber = 0 };
            dbContext.ProtocolSequences.Add(sequence);
        }

        sequence.LastNumber += 1;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (sequence.LastNumber, year);
    }
}
