using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IMovieService
{
  Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetMany(MovieSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default);
  Task<MovieExtendedDto> GetOne(Guid id, bool includePeople = false, CancellationToken token = default);
  Task<MovieDto> Create(MovieForChangeDto newMovie, CancellationToken token = default);
  Task Update(Guid id, MovieForChangeDto updatedMovie, CancellationToken token = default);
  Task Update(Guid id, JsonPatchDocument<MovieForChangeDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
