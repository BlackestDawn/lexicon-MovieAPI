using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Services;

public class PersonService : IPersonService
{
  public Task<PersonCreationResult> Create(PersonForCreationDto newPerson, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public Task<(IEnumerable<PersonDto>, PaginationMetadata?)> GetMany(PeopleSearchParams searchParams, int? page, int? pageSize, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<PersonDto> GetOne(Guid id, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public Task Remove(Guid id, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, PersonForUpdateDto updatedPerson, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, JsonPatchDocument<PersonForUpdateDto> patchDocument, CancellationToken token)
  {
    throw new NotImplementedException();
  }
}
