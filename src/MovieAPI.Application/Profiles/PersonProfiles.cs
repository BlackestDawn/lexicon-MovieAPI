using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Application.Models.V1;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class PersonProfiles : Profile
{
  public PersonProfiles()
  {
    CreateMap<Person, PersonDto>();
    CreateMap<PersonDto, PersonV1Dto>()
      .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.GivenName));

    CreateMap<Person, PersonForChangeDto>()
      .ForMember(dest => dest.MovieRoles, opt => opt.MapFrom(src => src.CastCrews));
    CreateMap<PersonForChangeV1Dto, PersonForChangeDto>()
      .ForMember(dest => dest.GivenName, opt => opt.MapFrom(src => src.FirstName))
      .ForMember(dest => dest.MiddleName, opt => opt.Ignore());

    CreateMap<CastCrew, CastCrewDto>()
      .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.Person.Id))
      .ForMember(dest => dest.GivenName, opt => opt.MapFrom(src => src.Person.GivenName))
      .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.Person.MiddleName))
      .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Person.LastName));
    CreateMap<CastCrewDto, CastCrewV1Dto>()
      .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.GivenName));

    CreateMap<CastCrew, MovieRoleDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Movie.Title));

    CreateMap<Person, PersonExtendedDto>()
      .ForMember(dest => dest.MovieRoles,
        opt => opt.MapFrom(src => src.CastCrews));

    CreateMap<PersonForChangeDto, Person>()
      .ForMember(dest => dest.CastCrews, opt => opt.Ignore());
    CreateMap<PersonForChangeV1Dto, PersonForChangeDto>()
      .ForMember(dest => dest.MiddleName, opt => opt.Ignore());

    CreateMap<MovieRoleForCreationDto, CastCrew>();
    CreateMap<CastCrew, MovieRoleForCreationDto>();
  }
}
