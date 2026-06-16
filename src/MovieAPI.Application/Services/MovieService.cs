using AutoMapper;
using Azure;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class MovieService(IMovieRepository repository, IMapper mapper) : IMovieService
{
  public Task<MovieCreationResult> Create(MovieForCreationDto newMovie, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetMany(MovieSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<MovieDto?> GetOne(Guid id, bool includePeople = false, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task Remove(Guid id, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, MovieForUpdateDto updatedMovie, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, JsonPatchDocument patchDocument, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }
}
