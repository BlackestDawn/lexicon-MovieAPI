using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class ReviewProfiles:Profile
{
  public ReviewProfiles()
  {
    CreateMap<Review, ReviewDto>();
  }
}
