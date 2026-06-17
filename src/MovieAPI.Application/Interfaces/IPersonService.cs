using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IPersonService
{
  Task<(IEnumerable<PersonDto>, PaginationMetadata?)> GetMany(PeopleSearchParams searchParams, int? page, int? pageSize, CancellationToken cancellationToken);
  Task<PersonDto> GetOne(Guid id, CancellationToken token);
  Task<PersonCreationResult> Create(PersonForCreationDto newPerson, CancellationToken token);
  Task<(bool, string?)> Update(Guid id, PersonForUpdateDto updatedPerson, CancellationToken token);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument<PersonForUpdateDto> patchDocument, CancellationToken token);
  Task Remove(Guid id, CancellationToken token);
}
