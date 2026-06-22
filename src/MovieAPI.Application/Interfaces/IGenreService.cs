using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.Interfaces;

public interface IGenreService
{
  Task<IEnumerable<GenreDto>> GetMany(CancellationToken token = default);
  Task<GenreExtendedDto?> GetOne(Guid id, bool includeMovies, CancellationToken token = default);
  Task<GenreCreationResult> Create(GenreForChangeDto newGenre, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, GenreForChangeDto updatedGenre, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument<GenreForChangeDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
