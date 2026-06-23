using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class MovieProfiles : Profile
{
  public MovieProfiles()
  {
    CreateMap<MovieDetail, MovieDetailDto>();

    CreateMap<Movie, MovieDto>()
      .ForMember(dest => dest.Genres,
        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre)))
      // AverageRating is computed in SQL by the repository (the list query doesn't load
      // Reviews), and assigned onto the DTO separately in MovieService.
      .ForMember(dest => dest.AverageRating, opt => opt.Ignore());

    CreateMap<Movie, MovieExtendedDto>()
      .ForMember(dest => dest.Genres,
        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre)))
      .ForMember(dest => dest.CastCrews,
        opt => opt.MapFrom(src => src.CastCrews));

    CreateMap<MovieForChangeDto, Movie>()
      .ForMember(dest => dest.Details,
        opt => opt.MapFrom(src => new MovieDetail
        {
          Synopsis = src.Synopsis,
          Language = src.Language,
          Budget = src.Budget
        }))
      .ForMember(dest => dest.CastCrews, opt => opt.Ignore())
      .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
      .ForMember(dest => dest.Reviews, opt => opt.Ignore());

    CreateMap<CastCrew, CastCrewForCreationDto>();
    CreateMap<CastCrewForCreationDto, CastCrew>();

    CreateMap<Movie, MovieForChangeDto>()
      .ForMember(dest => dest.Genres,
        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.GenreId)))
      .ForMember(dest => dest.Synopsis,
        opt => opt.MapFrom(src => src.Details.Synopsis))
      .ForMember(dest => dest.Language,
        opt => opt.MapFrom(src => src.Details.Language))
      .ForMember(dest => dest.Budget,
        opt => opt.MapFrom(src => src.Details.Budget));
  }
}
