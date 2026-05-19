namespace eProtocol.Application.Institutions;

public interface IInstitutionService
{
    Task<InstitutionDto> CreateAsync(CreateInstitutionRequest request, CancellationToken cancellationToken = default);
    Task<InstitutionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstitutionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InstitutionDto> UpdateAsync(Guid id, UpdateInstitutionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
