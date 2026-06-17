using Azure;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IMovieService
{
  Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetMany(MovieSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default);
  Task<MovieExtendedDto?> GetOne(Guid id, bool includePeople = false, CancellationToken token = default);
  Task<MovieCreationResult> Create(MovieForCreationDto newMovie, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, MovieForUpdateDto updatedMovie, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
