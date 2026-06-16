using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class MovieProfiles : Profile
{
  public MovieProfiles()
  {
    CreateMap<MovieDetail, MovieInfoDto>();

    CreateMap<Movie, MovieDto>()
      .ForMember(dest => dest.Genres,
        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre)));

    CreateMap<Movie, MovieDetailDto>()
      .ForMember(dest => dest.Genres,
        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre)))
      .ForMember(dest => dest.CastCrews,
        opt => opt.MapFrom(src => src.CastCrews));

    CreateMap<MovieForCreationDto, Movie>()
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
  }
}
