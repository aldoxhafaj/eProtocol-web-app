using AutoMapper;
using eProtocol.Application.Abstractions;
using eProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProtocol.Application.Institutions;

public class InstitutionService(IApplicationDbContext dbContext, IMapper mapper) : IInstitutionService
{
    public async Task<InstitutionDto> CreateAsync(CreateInstitutionRequest request, CancellationToken cancellationToken = default)
    {
        var institution = new Institution
        {
            Name = request.Name.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            Address = request.Address?.Trim()
        };

        dbContext.Institutions.Add(institution);
        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<InstitutionDto>(institution);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Institutions.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (institution is null)
        {
            return;
        }

        dbContext.Institutions.Remove(institution);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InstitutionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var institutions = await dbContext.Institutions.AsNoTracking().ToListAsync(cancellationToken);
        return institutions.Select(mapper.Map<InstitutionDto>).ToList();
    }

    public async Task<InstitutionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Institutions.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        return institution is null ? null : mapper.Map<InstitutionDto>(institution);
    }

    public async Task<InstitutionDto> UpdateAsync(Guid id, UpdateInstitutionRequest request, CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Institutions.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (institution is null)
        {
            throw new InvalidOperationException("Institution not found.");
        }

        institution.Name = request.Name.Trim();
        institution.ContactEmail = request.ContactEmail?.Trim();
        institution.ContactPhone = request.ContactPhone?.Trim();
        institution.Address = request.Address?.Trim();
        institution.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<InstitutionDto>(institution);
    }
}
