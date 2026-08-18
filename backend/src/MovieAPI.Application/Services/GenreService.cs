using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Services;

public class GenreService
  (IGenreRepository repository,
  IMovieRepository movieRepository,
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

  public async Task<(GenreExtendedDto, PaginationMetadata?)> GetOne(Guid id, bool includeMovies, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }
    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var result = await repository.GetGenreReadOnlyAsync(id, token) ?? throw new NotFoundException($"Genre '{id}' not found");
    var dto = mapper.Map<GenreExtendedDto>(result);

    PaginationMetadata? pagination = null;
    if (includeMovies)
    {
      var searchParams = new MovieSearchParams(null, null, result.Slug, null, null, null);
      var (movies, paginationData) = await movieRepository.GetMoviesReadOnlyAsync(searchParams, (int)page, (int)pageSize, token);

      dto.Movies = [.. movies.Select(item =>
      {
        var movie = mapper.Map<MovieSimpleDto>(item.Movie);
        movie.AverageRating = item.AverageRating;
        return movie;
      })];
      pagination = paginationData;
    }

    return (dto, pagination);
  }

  public async Task Remove(Guid id, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, token);
    if (entity == null)
    {
      return;
    }

    repository.Delete(entity);
    await repository.SaveChangesAsync(token);
  }

  public async Task Update(Guid id, GenreForChangeDto updatedGenre, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, token) ?? throw new NotFoundException($"Genre '{id}' not found");
    await ApplyUpdateAsync(entity, updatedGenre, token);
  }

  public async Task Update(Guid id, JsonPatchDocument<GenreForChangeDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetGenreAsync(id, token) ?? throw new NotFoundException($"Genre '{id}' not found");

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
