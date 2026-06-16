using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class GenreProfiles : Profile
{
  public GenreProfiles()
  {
    CreateMap<Genre, GenreDto>();
  }
}
