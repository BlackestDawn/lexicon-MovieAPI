using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class PersonProfiles : Profile
{
  public PersonProfiles()
  {
    CreateMap<Person, PersonDto>();
    CreateMap<Person, PersonForUpdateDto>()
      .ForMember(dest => dest.MovieRoles, opt => opt.MapFrom(src => src.CastCrews));

    CreateMap<CastCrew, CastCrewDto>()
      .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.Person.Id))
      .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Person.FirstName))
      .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Person.LastName));

    CreateMap<CastCrew, MovieRoleDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Movie.Title));

    CreateMap<Person, PersonExtendedDto>()
      .ForMember(dest => dest.MovieRoles,
        opt => opt.MapFrom(src => src.CastCrews));

    CreateMap<PersonForCreationDto, Person>()
      .ForMember(dest => dest.CastCrews, opt => opt.Ignore());

    CreateMap<MovieRoleForCreationDto, CastCrew>();
    CreateMap<CastCrew, MovieRoleForCreationDto>();
  }
}
