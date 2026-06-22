using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Application.Services;

public class GenreService
  (IGenreRepository repository,
  IMapper mapper,
  IValidator<GenreForChangeDto> validator) : IGenreService
{
  public async Task<GenreCreationResult> Create(GenreForChangeDto newGenre, CancellationToken token = default)
  {
    var validationResult = validator.Validate(newGenre);
    if (!validationResult.IsValid)
    {
      return GenreCreationResult.Failed(new ValidationException(validationResult.Errors));
    }

    var genreEntity = mapper.Map<Genre>(newGenre);

    await repository.AddAsync(genreEntity, token);
    await repository.SaveChangesAsync(token);

    return GenreCreationResult.Successful(mapper.Map<GenreDto>(genreEntity));
  }

  public async Task<IEnumerable<GenreDto>> GetMany(CancellationToken token = default)
  {
    var result = await repository.GetGenresReadOnlyAsync(token);

    return mapper.Map<IEnumerable<GenreDto>>(result);
  }

  public async Task<GenreExtendedDto?> GetOne(Guid id, bool includeMovies, CancellationToken token = default)
  {
    var result = await repository.GetGenreReadOnlyAsync(id, includeMovies, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<GenreExtendedDto>(result);
  }

  public async Task Remove(Guid id, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, false, token);
    if (entity == null)
    {
      return;
    }

    repository.Delete(entity);
    await repository.SaveChangesAsync(token);
  }

  public async Task<(bool, string?)> Update(Guid id, GenreForChangeDto updatedGenre, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, false, token);
    if (entity == null)
    {
      return (false, $"Genre '{id}' not found");
    }

    return await ApplyUpdateAsync(entity, updatedGenre, token);
  }

  public async Task<(bool, string?)> Update(Guid id, JsonPatchDocument<GenreForChangeDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, false, token);
    if (entity == null)
    {
      return (false, $"Genre '{id}' not found");
    }

    var dto = mapper.Map<GenreForChangeDto>(entity);
    patchDocument.ApplyTo(dto);

    return await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task<(bool, string?)> ApplyUpdateAsync(Genre entity, GenreForChangeDto updatedGenre, CancellationToken token)
  {
    var validationResult = validator.Validate(updatedGenre);
    if (!validationResult.IsValid)
    {
      return (false, new ValidationException(validationResult.Errors).Message);
    }

    entity.Name = updatedGenre.Name;
    entity.Slug = updatedGenre.Slug;

    await repository.SaveChangesAsync(token);

    return (true, null);
  }
}
