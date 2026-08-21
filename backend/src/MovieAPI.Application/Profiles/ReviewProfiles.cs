using AutoMapper;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Application.Profiles;

public class ReviewProfiles : Profile
{
  public ReviewProfiles()
  {
    // MovieTitle is only populated when the query loads the Movie navigation (the
    // cross-user "my reviews" listing does; the per-movie endpoints don't need to
    // and don't) - map explicitly instead of relying on flattening, which would
    // NRE on Review.Movie being null for the latter.
    CreateMap<Review, ReviewDto>()
      .ForMember(d => d.MovieTitle, opt => opt.MapFrom(s => s.Movie != null ? s.Movie.Title : string.Empty));
    CreateMap<ReviewForChangeDto, Review>();
    CreateMap<Review, ReviewForChangeDto>();
  }
}
