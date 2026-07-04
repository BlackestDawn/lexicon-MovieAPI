using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Exceptions;
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
  public async Task<GenreDto> Create(GenreForChangeDto newGenre, CancellationToken token = default)
  {
    var validationResult = await validator.ValidateAsync(newGenre, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var genreEntity = mapper.Map<Genre>(newGenre);

    await repository.AddAsync(genreEntity, token);
    await repository.SaveChangesAsync(token);

    return mapper.Map<GenreDto>(genreEntity);
  }

  public async Task<IEnumerable<GenreDto>> GetMany(CancellationToken token = default)
  {
    var result = await repository.GetGenresReadOnlyAsync(token);
    return mapper.Map<IEnumerable<GenreDto>>(result);
  }

  public async Task<GenreExtendedDto> GetOne(Guid id, bool includeMovies, CancellationToken token = default)
  {
    var result = await repository.GetGenreReadOnlyAsync(id, includeMovies, token) ?? throw new NotFoundException($"Genre '{id}' not found");
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

  public async Task Update(Guid id, GenreForChangeDto updatedGenre, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, false, token) ?? throw new NotFoundException($"Genre '{id}' not found");
    await ApplyUpdateAsync(entity, updatedGenre, token);
  }

  public async Task Update(Guid id, JsonPatchDocument<GenreForChangeDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, false, token) ?? throw new NotFoundException($"Genre '{id}' not found");

    var dto = mapper.Map<GenreForChangeDto>(entity);
    patchDocument.ApplyTo(dto);

    await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task ApplyUpdateAsync(Genre entity, GenreForChangeDto updatedGenre, CancellationToken token)
  {
    var validationResult = await validator.ValidateAsync(updatedGenre, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    entity.Name = updatedGenre.Name;
    entity.Slug = updatedGenre.Slug;

    await repository.SaveChangesAsync(token);
  }
}
