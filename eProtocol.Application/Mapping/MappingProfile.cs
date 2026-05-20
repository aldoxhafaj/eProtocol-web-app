using AutoMapper;
using eProtocol.Application.Documents;
using eProtocol.Application.Institutions;
using eProtocol.Application.Users;
using eProtocol.Domain.Entities;

namespace eProtocol.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Institution, InstitutionDto>();
        CreateMap<Document, DocumentDto>()
            .ForCtorParam("Assignments", opt => opt.MapFrom(src => src.Assignments));
        CreateMap<DocumentAssignment, DocumentAssignmentDto>()
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null));
    }
}
