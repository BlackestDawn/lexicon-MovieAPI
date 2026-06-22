using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public interface IMovieRepository
{
  // Movies
  Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesReadOnlyAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<Movie?> GetMovieAsync(Guid id, bool includePeople, CancellationToken cancellationToken);
  Task<bool> MovieExistsAsync(Guid id, CancellationToken cancellationToken);
  Task<IList<Guid>> GetMissingMovieIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken);
  Task AddMovieAsync(Movie movie, CancellationToken cancellationToken);
  void DeleteMovie(Movie movie);

  // Reviews
  Task<(IEnumerable<Review>, PaginationMetadata?)> GetReviewsForMovieAsync(Guid movieId, ReviewSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<Review?> GetReviewAsync(Guid movieId, Guid reviewId, CancellationToken cancellationToken);
  Task <bool> ReviewExistsAsync(Guid id, CancellationToken token);
  Task<IList<Guid>> GetMissingReviewIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken);
  Task AddReviewAsync(Guid movieId, Review review, CancellationToken cancellationToken);
  void DeleteReview(Review review);

  // People
  Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleAsync(PeopleSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleReadOnlyAsync(PeopleSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<Person?> GetPersonAsync(Guid id, bool includeMovies, CancellationToken cancellationToken);
  Task<bool> PersonExistsAsync(Guid id, CancellationToken cancellationToken);
  Task<IList<Guid>> GetMissingPersonIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken);
  Task AddPersonAsync(Person person, CancellationToken cancellationToken);
  void DeletePerson(Person person);

  // Genres
  Task<IEnumerable<Genre>> GetGenresAsync(CancellationToken cancellationToken);
  Task<Genre?> GetGenreAsync(Guid id, CancellationToken cancellationToken);
  Task<bool> GenreExistsAsync(Guid id, CancellationToken cancellationToken);
  Task<IList<Guid>> GetMissingGenreIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken);

  // Persistence
  Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
