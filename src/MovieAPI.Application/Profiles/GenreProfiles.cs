using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class GenreProfiles : Profile
{
  public GenreProfiles()
  {
    CreateMap<Genre, GenreDto>();

    CreateMap<Genre, GenreExtendedDto>()
      .ForMember(dest => dest.Movies,
        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Movie)));

    CreateMap<GenreForChangeDto, Genre>();
    CreateMap<Genre, GenreForChangeDto>();
  }
}
