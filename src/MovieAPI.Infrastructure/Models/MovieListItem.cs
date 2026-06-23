using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.Models;

public record MovieListItem(Movie Movie, decimal AverageRating);
