using AutoMapper;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class GenreService
  (IMovieRepository repository,
  IMapper mapper) : IGenreService
{
  public async Task<IEnumerable<GenreDto>> GetMany(CancellationToken token)
  {
    var result = await repository.GetGenresAsync(token);

    return mapper.Map<IEnumerable<GenreDto>>(result);
  }

  public async Task<GenreDto?> GetOne(Guid id, CancellationToken token)
  {
    var result = await repository.GetGenreAsync(id, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<GenreDto>(result);
  }
}
