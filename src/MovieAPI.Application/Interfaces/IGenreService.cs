using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.Interfaces;

public interface IGenreService
{
  Task<IEnumerable<GenreDto>> GetMany(CancellationToken token);
  Task<GenreExtendedDto?> GetOne(Guid id, bool includeMovies, CancellationToken token);
  Task<GenreCreationResult> Create(GenreForChangeDto newGenre, CancellationToken token);
  Task<(bool, string?)> Update(Guid id, GenreForChangeDto updatedGenre, CancellationToken token);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument<GenreForChangeDto> patchDocument, CancellationToken token);
}
