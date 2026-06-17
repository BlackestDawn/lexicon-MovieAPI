using MovieAPI.Application.Models;

namespace MovieAPI.Application.Interfaces;

public interface IGenreService
{
  Task<IEnumerable<GenreDto>> GetMany(CancellationToken token);
  Task<GenreDto?> GetOne(Guid id, CancellationToken token);
}
