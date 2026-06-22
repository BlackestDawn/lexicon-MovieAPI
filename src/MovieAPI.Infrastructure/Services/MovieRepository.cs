using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public class MovieRepository(AppDbContext context) : IMovieRepository
{
  // Movies
  public async Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return await GetMoviesInternalAsync(searchParams, page, pageSize, false, cancellationToken);
  }

  public async Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesReadOnlyAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return await GetMoviesInternalAsync(searchParams, page, pageSize, true, cancellationToken);
  }

  private async Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesInternalAsync(MovieSearchParams searchParams, int page, int pageSize, bool readOnly, CancellationToken cancellationToken)
  {
    var query = context.Movies.AsQueryable();

    if (readOnly)
    {
      query = query.AsNoTracking();
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Name))
    {
      query = query.Where(m => m.Title.Contains(searchParams.Name));
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Search))
    {
      query = query.Where(m => m.Title.Contains(searchParams.Search) || m.PlotSummery.Contains(searchParams.Search));
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Genre))
    {
      query = query.Where(m => m.MovieGenres.Any(mg => mg.Genre.Slug == searchParams.Genre || mg.Genre.Name == searchParams.Genre));
    }

    if (searchParams.Year.HasValue)
    {
      query = query.Where(m => m.ReleaseDate.Year == searchParams.Year.Value);
    }

    if (searchParams.MinRating.HasValue)
    {
      query = query.Where(m => m.Reviews.Average(r => (double?)r.Score) >= (double)searchParams.MinRating.Value);
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var pagination = new PaginationMetadata(totalCount, pageSize, page);

    var movies = await query
      .OrderBy(m => m.Title)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
      .Include(m => m.Reviews)
      .ToListAsync(cancellationToken);

    return (movies, pagination);
  }

  public async Task<Movie?> GetMovieAsync(Guid id, bool includePeople, CancellationToken cancellationToken)
  {
    var query = context.Movies
      .Include(m => m.Details)
      .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
      .Include(m => m.Reviews)
      .AsQueryable();

    if (includePeople)
    {
      query = query.Include(m => m.CastCrews).ThenInclude(cc => cc.Person);
    }

    return await query.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
  }

  public async Task<bool> MovieExistsAsync(Guid id, CancellationToken cancellationToken)
  {
    return await context.Movies.AnyAsync(m => m.Id == id, cancellationToken);
  }

  public async Task<IList<Guid>> GetMissingMovieIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken)
  {
    var found = await context.Movies
      .Where(m => ids.Contains(m.Id))
      .Select(m => m.Id)
      .ToListAsync(cancellationToken);
    return [..ids.Except(found)];
  }

  public async Task AddMovieAsync(Movie movie, CancellationToken cancellationToken)
  {
    await context.Movies.AddAsync(movie, cancellationToken);
  }

  public void DeleteMovie(Movie movie)
  {
    context.Movies.Remove(movie);
  }

  // Reviews
  public async Task<(IEnumerable<Review>, PaginationMetadata?)> GetReviewsForMovieAsync(Guid movieId, ReviewSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    var query = context.Reviews.AsQueryable();
    query = query.Where(r => r.MovieId == movieId);

    if (!string.IsNullOrWhiteSpace(searchParams.Search))
    {
      query = query.Where(r => r.Body.Contains(searchParams.Search));
    }

    if (searchParams.MinScore.HasValue)
    {
      query = query.Where(r => r.Score >= (int)searchParams.MinScore);
    }

    if (searchParams.MaxScore.HasValue)
    {
      query = query.Where(r => r.Score <= (int)searchParams.MaxScore);
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var pagination = new PaginationMetadata(totalCount, pageSize, page);

    var reviews = await query
      .OrderBy(r => r.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (reviews, pagination);
  }

  public async Task<Review?> GetReviewAsync(Guid movieId, Guid reviewId, CancellationToken cancellationToken)
  {
    return await context.Reviews
      .FirstOrDefaultAsync(r => r.MovieId == movieId && r.Id == reviewId, cancellationToken);
  }

  public Task<bool> ReviewExistsAsync(Guid id, CancellationToken token)
  {
    throw new NotImplementedException();
  }

  public Task<IList<Guid>> GetMissingReviewIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task AddReviewAsync(Guid movieId, Review review, CancellationToken cancellationToken)
  {
    review.MovieId = movieId;
    await context.Reviews.AddAsync(review, cancellationToken);
  }

  public void DeleteReview(Review review)
  {
    context.Reviews.Remove(review);
  }

  // People
  public async Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleAsync(PeopleSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return await GetPeopleInternalAsync(searchParams, page, pageSize, false, cancellationToken);
  }

  public async Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleReadOnlyAsync(PeopleSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return await GetPeopleInternalAsync(searchParams, page, pageSize, true, cancellationToken);
  }

  private async Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleInternalAsync(PeopleSearchParams searchParams, int page, int pageSize, bool readOnly, CancellationToken cancellationToken)
  {
    var query = context.Persons.AsQueryable();

    if (readOnly)
    {
      query = query.AsNoTracking();
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Name))
    {
      query = query.Where(p => p.FirstName.Contains(searchParams.Name) || p.LastName.Contains(searchParams.Name));
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Genre))
    {
      query = query.Where(p => p.CastCrews.Any(cc => cc.Movie.MovieGenres.Any(mg => mg.Genre.Slug == searchParams.Genre || mg.Genre.Name == searchParams.Genre)));
    }

    if (searchParams.Year.HasValue)
    {
      query = query.Where(p => p.CastCrews.Any(cc => cc.Movie.ReleaseDate.Year == searchParams.Year.Value));
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var pagination = new PaginationMetadata(totalCount, pageSize, page);

    var people = await query
      .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (people, pagination);
  }

  public async Task<Person?> GetPersonAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    var query = context.Persons.AsQueryable();

    if (includeMovies)
    {
      query = query.Include(p => p.CastCrews).ThenInclude(cc => cc.Movie);
    }

    return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }

  public async Task<bool> PersonExistsAsync(Guid id, CancellationToken cancellationToken)
  {
    return await context.Persons.AnyAsync(p => p.Id == id, cancellationToken);
  }

  public async Task<IList<Guid>> GetMissingPersonIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken)
  {
    var found = await context.Persons
      .Where(p => ids.Contains(p.Id))
      .Select(p => p.Id)
      .ToListAsync(cancellationToken);
    return [..ids.Except(found)];
  }

  public async Task AddPersonAsync(Person person, CancellationToken cancellationToken)
  {
    await context.Persons.AddAsync(person, cancellationToken);
  }

  public void DeletePerson(Person person)
  {
    context.Persons.Remove(person);
  }

  // Genres
  public async Task<IEnumerable<Genre>> GetGenresAsync(CancellationToken cancellationToken)
  {
    return await context.Genres.AsNoTracking().ToListAsync(cancellationToken);
  }

  public async Task<Genre?> GetGenreAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    var query = context.Genres.AsQueryable();

    if (includeMovies)
    {
      query = query.Include(g => g.MovieGenres).ThenInclude(gm => gm.Movie);
    }

    return await query.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
  }

  public async Task<bool> GenreExistsAsync(Guid id, CancellationToken cancellationToken)
  {
    return await context.Genres.AnyAsync(g => g.Id == id, cancellationToken);
  }

  public async Task<IList<Guid>> GetMissingGenreIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken)
  {
    var found = await context.Genres
      .Where(g => ids.Contains(g.Id))
      .Select(g => g.Id)
      .ToListAsync(cancellationToken);
    return [..ids.Except(found)];
  }

  // Persistence
  public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return await context.SaveChangesAsync(cancellationToken) > 0;
  }
}
