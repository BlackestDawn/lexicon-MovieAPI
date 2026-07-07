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
      // Movies is populated separately in GenreService via the paginated movie search
      // (which also computes AverageRating in SQL), not from the Genre entity.
      .ForMember(dest => dest.Movies, opt => opt.Ignore());

    CreateMap<GenreForChangeDto, Genre>();
    CreateMap<Genre, GenreForChangeDto>();
  }
}
