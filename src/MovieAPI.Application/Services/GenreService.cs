using AutoMapper;
using FluentValidation;
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
  public async Task<GenreCreationResult> Create(GenreForChangeDto newGenre, CancellationToken token)
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

  public async Task<IEnumerable<GenreDto>> GetMany(CancellationToken token)
  {
    var result = await repository.GetGenresAsync(token);

    return mapper.Map<IEnumerable<GenreDto>>(result);
  }

  public async Task<GenreExtendedDto?> GetOne(Guid id, bool includeMovies, CancellationToken token)
  {
    var result = await repository.GetGenreAsync(id, includeMovies, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<GenreExtendedDto>(result);
  }
}
