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

public class MovieService(
  IMovieRepository repository,
  IPersonRepository personRepository,
  IGenreRepository genreRepository,
  IMapper mapper,
  IValidator<MovieForChangeDto> validator) : IMovieService
{
  public async Task<MovieDto> Create(MovieForChangeDto newMovie, CancellationToken token = default)
  {
    var validationResult = await validator.ValidateAsync(newMovie, token);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var invalidPersonIds = await personRepository.GetMissingIdsAsync(
      newMovie.CastCrews.Select(cc => cc.PersonId).Distinct().ToList(), token);
    var invalidGenreIds = await genreRepository.GetMissingIdsAsync(
      newMovie.Genres.Distinct().ToList(), token);

    if (invalidPersonIds.Count > 0 || invalidGenreIds.Count > 0)
    {
      var errors = invalidPersonIds.Select(id => $"Person '{id}' not found")
        .Concat(invalidGenreIds.Select(id => $"Genre '{id}' not found"));
      throw new NotFoundException(string.Join("; ", errors));
    }

    var movieEntity = mapper.Map<Movie>(newMovie);

    movieEntity.CastCrews = mapper.Map<ICollection<CastCrew>>(newMovie.CastCrews);

    movieEntity.MovieGenres = [..newMovie.Genres
      .Select(genreId => new MovieGenre { GenreId = genreId })];

    await repository.AddAsync(movieEntity, token);
    await repository.SaveChangesAsync(token);

    // refetch to properly populate genres; read-only since the result is only used to build the response
    var savedMovie = await repository.GetMovieReadOnlyAsync(movieEntity.Id, false, token);
    return mapper.Map<MovieDto>(savedMovie);
  }

  public async Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetMany(MovieSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }
    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var (result, pagination) = await repository.GetMoviesReadOnlyAsync(searchParams, (int)page, (int)pageSize, token);

    var movies = result.Select(item =>
    {
      var dto = mapper.Map<MovieDto>(item.Movie);
      dto.AverageRating = item.AverageRating;
      return dto;
    });

    return (movies, pagination);
  }

  public async Task<MovieExtendedDto> GetOne(Guid id, bool includePeople = false, CancellationToken token = default)
  {
    var result = await repository.GetMovieReadOnlyAsync(id, includePeople, token) ?? throw new NotFoundException($"Movie '{id}' not found");
    return mapper.Map<MovieExtendedDto>(result);
  }

  public async Task Remove(Guid id, CancellationToken token = default)
  {
    var entity = await repository.GetMovieAsync(id, false, token);
    if (entity == null)
    {
      return;
    }

    repository.Delete(entity);
    await repository.SaveChangesAsync(token);
  }

  public async Task Update(Guid id, MovieForChangeDto updatedMovie, CancellationToken token = default)
  {
    var entity = await repository.GetMovieAsync(id, true, token) ?? throw new NotFoundException($"Movie '{id}' not found");
    await ApplyUpdateAsync(entity, updatedMovie, token);
  }

  public async Task Update(Guid id, JsonPatchDocument<MovieForChangeDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetMovieAsync(id, true, token) ?? throw new NotFoundException($"Movie '{id}' not found");

    var dto = mapper.Map<MovieForChangeDto>(entity);
    patchDocument.ApplyTo(dto);

    await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task ApplyUpdateAsync(Movie entity, MovieForChangeDto updatedMovie, CancellationToken token)
  {
    var validationResult = await validator.ValidateAsync(updatedMovie, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var invalidPersonIds = await personRepository.GetMissingIdsAsync(
      updatedMovie.CastCrews.Select(cc => cc.PersonId).Distinct().ToList(), token);
    var invalidGenreIds = await genreRepository.GetMissingIdsAsync(
      updatedMovie.Genres.Distinct().ToList(), token);

    if (invalidPersonIds.Count > 0 || invalidGenreIds.Count > 0)
    {
      var errors = invalidPersonIds.Select(id => $"Person '{id}' not found")
        .Concat(invalidGenreIds.Select(id => $"Genre '{id}' not found"));
      throw new NotFoundException(string.Join("; ", errors));
    }

    entity.Title = updatedMovie.Title;
    entity.ReleaseDate = updatedMovie.ReleaseDate;
    entity.PlotSummery = updatedMovie.PlotSummery;
    entity.RuntimeMinutes = updatedMovie.RuntimeMinutes;

    entity.CastCrews.Clear();
    foreach (var cc in mapper.Map<ICollection<CastCrew>>(updatedMovie.CastCrews))
      entity.CastCrews.Add(cc);

    entity.MovieGenres.Clear();
    foreach (var genreId in updatedMovie.Genres)
      entity.MovieGenres.Add(new MovieGenre { GenreId = genreId });

    entity.Details.Synopsis = updatedMovie.Synopsis;
    entity.Details.Language = updatedMovie.Language;
    entity.Details.Budget = updatedMovie.Budget;

    await repository.SaveChangesAsync(token);
  }
}
