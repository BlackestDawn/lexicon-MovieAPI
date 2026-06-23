using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IPersonService
{
  Task<(IEnumerable<PersonDto>, PaginationMetadata?)> GetMany(PeopleSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default);
  Task<PersonExtendedDto> GetOne(Guid id, bool includeMovies, CancellationToken token = default);
  Task<PersonDto> Create(PersonForChangeDto newPerson, CancellationToken token = default);
  Task Update(Guid id, PersonForChangeDto updatedPerson, CancellationToken token = default);
  Task Update(Guid id, JsonPatchDocument<PersonForChangeDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
