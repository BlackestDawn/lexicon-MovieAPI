using AutoMapper;
using Azure;
using MovieAPI.Application.Helpers;
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

  public async Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetMany(MovieSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }
    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var (result, pagination) = await repository.GetMoviesReadOnlyAsync(searchParams, (int)page, (int)pageSize, token);

    return (mapper.Map<IEnumerable<MovieDto>>(result), pagination);
  }

  public async Task<MovieExtendedDto?> GetOne(Guid id, bool includePeople = false, CancellationToken token = default)
  {
    var result = await repository.GetMovieAsync(id, includePeople, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<MovieExtendedDto>(result);
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
