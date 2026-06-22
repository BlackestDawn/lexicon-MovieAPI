using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IPersonService
{
  Task<(IEnumerable<PersonDto>, PaginationMetadata?)> GetMany(PeopleSearchParams searchParams, int? page, int? pageSize, CancellationToken cancellationToken);
  Task<PersonExtendedDto?> GetOne(Guid id, bool includeMovies, CancellationToken token);
  Task<PersonCreationResult> Create(PersonForChangeDto newPerson, CancellationToken token);
  Task<(bool, string?)> Update(Guid id, PersonForChangeDto updatedPerson, CancellationToken token);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument<PersonForChangeDto> patchDocument, CancellationToken token);
  Task Remove(Guid id, CancellationToken token);
}
