using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class PersonService(
  IMovieRepository repository,
  IMapper mapper,
  IValidator<PersonForCreationDto> createValidator,
  IValidator<PersonForUpdateDto> updateValidator
) : IPersonService
{
  public Task<PersonCreationResult> Create(PersonForCreationDto newPerson, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public async Task<(IEnumerable<PersonDto>, PaginationMetadata?)> GetMany(PeopleSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }
    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var (result, pagination) = await repository.GetPeopleReadOnlyAsync(searchParams, (int)page, (int)pageSize, token);

    return (mapper.Map<IEnumerable<PersonDto>>(result), pagination);
  }

  public async Task<PersonExtendedDto?> GetOne(Guid id, bool includeMovies, CancellationToken token = default)
  {
    var result = await repository.GetPersonAsync(id, includeMovies, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<PersonExtendedDto>(result);
  }

  public Task Remove(Guid id, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, PersonForUpdateDto updatedPerson, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, JsonPatchDocument<PersonForUpdateDto> patchDocument, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }
}
