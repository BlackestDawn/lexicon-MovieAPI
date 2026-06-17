using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class MovieService(
  IMovieRepository repository,
  IMapper mapper,
  IValidator<MovieForCreationDto> createValidator) : IMovieService
{
  public async Task<MovieCreationResult> Create(MovieForCreationDto newMovie, CancellationToken token = default)
  {
    var validationResult = createValidator.Validate(newMovie);

    if (!validationResult.IsValid)
    {
      var error = new ValidationException(validationResult.Errors);
      return MovieCreationResult.Failed(error);
    }

    var personIds = newMovie.CastCrews.Select(cc => cc.PersonId).Distinct().ToList();
    var personExistsFlags = await Task.WhenAll(personIds.Select(id => repository.PersonExistsAsync(id, token)));
    var invalidPersonIds = personIds.Where((_, i) => !personExistsFlags[i]).ToList();

    var genreIds = newMovie.Genres.Distinct().ToList();
    var genreExistsFlags = await Task.WhenAll(genreIds.Select(id => repository.GenreExistsAsync(id, token)));
    var invalidGenreIds = genreIds.Where((_, i) => !genreExistsFlags[i]).ToList();

    if (invalidPersonIds.Count > 0 || invalidGenreIds.Count > 0)
    {
      var errors = invalidPersonIds.Select(id => $"Person '{id}' not found")
        .Concat(invalidGenreIds.Select(id => $"Genre '{id}' not found"));
      return MovieCreationResult.Failed(new ArgumentException(string.Join("; ", errors)));
    }

    var movieEntity = mapper.Map<Movie>(newMovie);

    movieEntity.CastCrews = [..newMovie.CastCrews
      .Select(cc => new CastCrew { PersonId = cc.PersonId, Role = cc.Role })];

    movieEntity.MovieGenres = [..newMovie.Genres
      .Select(genreId => new MovieGenre { GenreId = genreId })];

    await repository.AddMovieAsync(movieEntity, token);
    await repository.SaveChangesAsync(token);

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
    var result = await repository.GetMovieAsync(id, includePeople, token);

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

    repository.DeleteMovie(entity);
    await repository.SaveChangesAsync(token);
  }

  public Task<(bool, string?)> Update(Guid id, MovieForUpdateDto updatedMovie, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, JsonPatchDocument<MovieForUpdateDto> patchDocument, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }
}
