using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class MovieService(
  IMovieRepository repository,
  IPersonRepository personRepository,
  IGenreRepository genreRepository,
  IMapper mapper,
  IValidator<MovieForCreationDto> createValidator,
  IValidator<MovieForUpdateDto> updateValidator) : IMovieService
{
  public async Task<MovieCreationResult> Create(MovieForCreationDto newMovie, CancellationToken token = default)
  {
    var validationResult = createValidator.Validate(newMovie);

    if (!validationResult.IsValid)
    {
      var error = new ValidationException(validationResult.Errors);
      return MovieCreationResult.Failed(error);
    }

    var invalidPersonIds = await personRepository.GetMissingIdsAsync(
      newMovie.CastCrews.Select(cc => cc.PersonId).Distinct().ToList(), token);
    var invalidGenreIds = await genreRepository.GetMissingIdsAsync(
      newMovie.Genres.Distinct().ToList(), token);

    if (invalidPersonIds.Count > 0 || invalidGenreIds.Count > 0)
    {
      var errors = invalidPersonIds.Select(id => $"Person '{id}' not found")
        .Concat(invalidGenreIds.Select(id => $"Genre '{id}' not found"));
      return MovieCreationResult.Failed(new ArgumentException(string.Join("; ", errors)));
    }

    var movieEntity = mapper.Map<Movie>(newMovie);

    movieEntity.CastCrews = mapper.Map<ICollection<CastCrew>>(newMovie.CastCrews);

    movieEntity.MovieGenres = [..newMovie.Genres
      .Select(genreId => new MovieGenre { GenreId = genreId })];

    await repository.AddAsync(movieEntity, token);
    await repository.SaveChangesAsync(token);

    // refetch to properly populate genres
    var savedMovie = await repository.GetMovieAsync(movieEntity.Id, false, token);
    return MovieCreationResult.Successful(mapper.Map<MovieDto>(savedMovie));
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

    return (mapper.Map<IEnumerable<MovieDto>>(result), pagination);
  }

  public async Task<MovieExtendedDto?> GetOne(Guid id, bool includePeople = false, CancellationToken token = default)
  {
    var result = await repository.GetMovieReadOnlyAsync(id, includePeople, token);

    if (result == null)
    {
      return null;
    }

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

  public async Task<(bool, string?)> Update(Guid id, MovieForUpdateDto updatedMovie, CancellationToken token = default)
  {
    var entity = await repository.GetMovieAsync(id, true, token);
    if (entity == null)
      return (false, $"Movie '{id}' not found");

    return await ApplyUpdateAsync(entity, updatedMovie, token);
  }

  public async Task<(bool, string?)> Update(Guid id, JsonPatchDocument<MovieForUpdateDto> patchDocument, CancellationToken token = default)
  {
    var entity = await repository.GetMovieAsync(id, true, token);
    if (entity == null)
      return (false, $"Movie '{id}' not found");

    var dto = mapper.Map<MovieForUpdateDto>(entity);
    patchDocument.ApplyTo(dto);

    return await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task<(bool, string?)> ApplyUpdateAsync(Movie entity, MovieForUpdateDto updatedMovie, CancellationToken token)
  {
    var validationResult = updateValidator.Validate(updatedMovie);
    if (!validationResult.IsValid)
      return (false, new ValidationException(validationResult.Errors).Message);

    var invalidPersonIds = await personRepository.GetMissingIdsAsync(
      updatedMovie.CastCrews.Select(cc => cc.PersonId).Distinct().ToList(), token);
    var invalidGenreIds = await genreRepository.GetMissingIdsAsync(
      updatedMovie.Genres.Distinct().ToList(), token);

    if (invalidPersonIds.Count > 0 || invalidGenreIds.Count > 0)
    {
      var errors = invalidPersonIds.Select(id => $"Person '{id}' not found")
        .Concat(invalidGenreIds.Select(id => $"Genre '{id}' not found"));
      return (false, string.Join("; ", errors));
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
    return (true, null);
  }
}
