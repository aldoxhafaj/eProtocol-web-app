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
            sequence = new Domain.Entities.ProtocolSequence { Year = year, StartNumber = 1, EndNumber = int.MaxValue, LastNumber = 0 };
            dbContext.ProtocolSequences.Add(sequence);
        }

        var next = sequence.LastNumber == 0 ? sequence.StartNumber : sequence.LastNumber + 1;

        if (next > sequence.EndNumber)
        {
            throw new InvalidOperationException($"Protocol number series exhausted for year {year}.");
        }

        sequence.LastNumber = next;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (next, year);
    }
}
