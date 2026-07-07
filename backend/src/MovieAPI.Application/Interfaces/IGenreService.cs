using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IGenreService
{
  Task<IEnumerable<GenreDto>> GetMany(CancellationToken token = default);
  Task<(GenreExtendedDto, PaginationMetadata?)> GetOne(Guid id, bool includeMovies, int? page, int? pageSize, CancellationToken token = default);
  Task<GenreDto> Create(GenreForChangeDto newGenre, CancellationToken token = default);
  Task Update(Guid id, GenreForChangeDto updatedGenre, CancellationToken token = default);
  Task Update(Guid id, JsonPatchDocument<GenreForChangeDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
