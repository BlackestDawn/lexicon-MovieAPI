using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Models.V1;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Services;

public class PersonService(
  IPersonRepository repository,
  IMovieRepository movieRepository,
  IMapper mapper,
  IValidator<PersonForChangeDto> validator
) : IPersonService
{
  public async Task<PersonDto> Create(PersonForChangeDto newPerson, CancellationToken token = default)
  {
    var validationResult = await validator.ValidateAsync(newPerson, token);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var movieIds = newPerson.MovieRoles.Select(mr => mr.MovieId).Distinct().ToList();
    var invalidMovieIds = await movieRepository.GetMissingIdsAsync(movieIds, token);

    if (invalidMovieIds.Count > 0)
    {
      var errors = invalidMovieIds.Select(id => $"Movie '{id}' not found");
      throw new NotFoundException(string.Join("; ", errors));
    }

    var personEntity = mapper.Map<Person>(newPerson);

    personEntity.CastCrews = mapper.Map<ICollection<CastCrew>>(newPerson.MovieRoles);

    await repository.AddAsync(personEntity, token);
    await repository.SaveChangesAsync(token);

    return mapper.Map<PersonDto>(personEntity);
  }

  public async Task<(IEnumerable<PersonDto>, PaginationMetadata?)> GetMany(PersonSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }
    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var (result, pagination) = await repository.GetPersonsReadOnlyAsync(searchParams, (int)page, (int)pageSize, token);

    return (mapper.Map<IEnumerable<PersonDto>>(result), pagination);
  }

  public async Task<PersonExtendedDto> GetOne(Guid id, bool includeMovies, CancellationToken token = default)
  {
    var result = await repository.GetPersonReadOnlyAsync(id, includeMovies, token) ?? throw new NotFoundException($"Person '{id}' not found");
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

  public async Task Update(Guid id, PersonForChangeDto updatedPerson, CancellationToken token = default)
  {
    var entity = await repository.GetPersonAsync(id, true, token) ?? throw new NotFoundException($"Person '{id}' not found");
    await ApplyUpdateAsync(entity, updatedPerson, token);
  }

  public async Task Update(Guid id, JsonPatchDocument<PersonForChangeDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetPersonAsync(id, true, token) ?? throw new NotFoundException($"Person '{id}' not found");

    var dto = mapper.Map<PersonForChangeDto>(entity);
    patchDocument.ApplyTo(dto);

    await ApplyUpdateAsync(entity, dto, token);
  }

  // Patches against the V1-shaped (FirstName, no MiddleName) view of the resource, then
  // merges the result back onto the current canonical state - mapping onto an existing
  // dto instance leaves ignored members (MiddleName) untouched, so a V1 patch that never
  // mentions MiddleName can't wipe out a value set through V2.
  public async Task Update(Guid id, JsonPatchDocument<PersonForChangeV1Dto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetPersonAsync(id, true, token) ?? throw new NotFoundException($"Person '{id}' not found");

    var dto = mapper.Map<PersonForChangeDto>(entity);
    var v1Dto = mapper.Map<PersonForChangeV1Dto>(dto);
    patchDocument.ApplyTo(v1Dto);
    mapper.Map(v1Dto, dto);

    await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task ApplyUpdateAsync(Person entity, PersonForChangeDto updatedPerson, CancellationToken token)
  {
    var validationResult = await validator.ValidateAsync(updatedPerson, token);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var movieIds = updatedPerson.MovieRoles.Select(mr => mr.MovieId).Distinct().ToList();
    var invalidMovieIds = await movieRepository.GetMissingIdsAsync(movieIds, token);

    if (invalidMovieIds.Count > 0)
    {
      var errors = invalidMovieIds.Select(id => $"Movie '{id}' not found");
      throw new NotFoundException(string.Join("; ", errors));
    }

    entity.GivenName = updatedPerson.GivenName;
    entity.MiddleName = updatedPerson.MiddleName;
    entity.LastName = updatedPerson.LastName;
    entity.DateOfBirth = updatedPerson.DateOfBirth;

    entity.CastCrews.Clear();
    foreach (var cc in mapper.Map<ICollection<CastCrew>>(updatedPerson.MovieRoles))
      entity.CastCrews.Add(cc);

    await repository.SaveChangesAsync(token);
  }
}
