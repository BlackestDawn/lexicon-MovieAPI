using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Helpers;
using MovieAPI.Domain.Entities;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Application.Services;

public class PersonService(
  IPersonRepository repository,
  IMovieRepository movieRepository,
  IMapper mapper,
  IValidator<PersonForCreationDto> createValidator,
  IValidator<PersonForUpdateDto> updateValidator
) : IPersonService
{
  public async Task<PersonCreationResult> Create(PersonForCreationDto newPerson, CancellationToken token = default)
  {
    var validationResult = createValidator.Validate(newPerson);

    if (!validationResult.IsValid)
    {
      return PersonCreationResult.Failed(new ValidationException(validationResult.Errors));
    }

    var movieIds = newPerson.MovieRoles.Select(mr => mr.MovieId).Distinct().ToList();
    var invalidMovieIds = await movieRepository.GetMissingIdsAsync(movieIds, token);

    if (invalidMovieIds.Count > 0)
    {
      var errors = invalidMovieIds.Select(id => $"Movie '{id}' not found");
      return PersonCreationResult.Failed(new ArgumentException(string.Join("; ", errors)));
    }

    var personEntity = mapper.Map<Person>(newPerson);

    personEntity.CastCrews = mapper.Map<ICollection<CastCrew>>(newPerson.MovieRoles);

    await repository.AddAsync(personEntity, token);
    await repository.SaveChangesAsync(token);

    return PersonCreationResult.Successful(mapper.Map<PersonDto>(personEntity));
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
    var result = await repository.GetPersonReadOnlyAsync(id, includeMovies, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<PersonExtendedDto>(result);
  }

  public async Task Remove(Guid id, CancellationToken token = default)
  {
    var entity = await repository.GetPersonAsync(id, false, token);
    if (entity == null)
    {
      return;
    }

    repository.Delete(entity);
    await repository.SaveChangesAsync(token);
  }

  public async Task<(bool, string?)> Update(Guid id, PersonForUpdateDto updatedPerson, CancellationToken token = default)
  {
    var entity = await repository.GetPersonAsync(id, true, token);
    if (entity == null)
    {
      return (false, $"Person '{id}' not found");
    }

    return await ApplyUpdateAsync(entity, updatedPerson, token);
  }

  public async Task<(bool, string?)> Update(Guid id, JsonPatchDocument<PersonForUpdateDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetPersonAsync(id, true, token);
    if (entity == null)
    {
      return (false, $"Person '{id}' not found");
    }

    var dto = mapper.Map<PersonForUpdateDto>(entity);
    patchDocument.ApplyTo(dto);

    return await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task<(bool, string?)> ApplyUpdateAsync(Person entity, PersonForUpdateDto updatedPerson, CancellationToken token)
  {
    var validationResult = updateValidator.Validate(updatedPerson);

    if (!validationResult.IsValid)
    {
      return (false, new ValidationException(validationResult.Errors).Message);
    }

    var movieIds = updatedPerson.MovieRoles.Select(mr => mr.MovieId).Distinct().ToList();
    var invalidMovieIds = await movieRepository.GetMissingIdsAsync(movieIds, token);

    if (invalidMovieIds.Count > 0)
    {
      var errors = invalidMovieIds.Select(id => $"Movie '{id}' not found");
      return (false, string.Join("; ", errors));
    }

    entity.FirstName = updatedPerson.FirstName;
    entity.LastName = updatedPerson.LastName;
    entity.DateOfBirth = updatedPerson.DateOfBirth;

    entity.CastCrews.Clear();
    foreach (var cc in mapper.Map<ICollection<CastCrew>>(updatedPerson.MovieRoles))
      entity.CastCrews.Add(cc);

    await repository.SaveChangesAsync(token);
    return (true, null);
  }
}
