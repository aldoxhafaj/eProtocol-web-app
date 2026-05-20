namespace eProtocol.Application.Institutions;

public record InstitutionDto(Guid Id, string Name, string? ContactEmail, string? ContactPhone, string? Address, bool IsActive);

public record CreateInstitutionRequest(string Name, string? ContactEmail, string? ContactPhone, string? Address);

public record UpdateInstitutionRequest(string Name, string? ContactEmail, string? ContactPhone, string? Address, bool IsActive);
