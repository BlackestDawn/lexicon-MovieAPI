using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class AuthProfiles : Profile
{
  public AuthProfiles()
  {
    CreateMap<ApplicationUser, UserDto>();
  }
}
