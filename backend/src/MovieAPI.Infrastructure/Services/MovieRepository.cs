using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public class MovieRepository(AppDbContext context) : RepositoryBase<Movie>(context), IMovieRepository
{
  protected override DbSet<Movie> Set => Context.Movies;

  public async Task<(IEnumerable<MovieListItem>, PaginationMetadata?)> GetMoviesAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return await GetMoviesInternalAsync(searchParams, page, pageSize, false, cancellationToken);
  }

  public async Task<(IEnumerable<MovieListItem>, PaginationMetadata?)> GetMoviesReadOnlyAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return await GetMoviesInternalAsync(searchParams, page, pageSize, true, cancellationToken);
  }

  private async Task<(IEnumerable<MovieListItem>, PaginationMetadata?)> GetMoviesInternalAsync(MovieSearchParams searchParams, int page, int pageSize, bool readOnly, CancellationToken cancellationToken)
  {
    var query = Context.Movies.AsQueryable();

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
      var yearStart = new DateOnly(searchParams.Year.Value, 1, 1);
      var yearEnd = yearStart.AddYears(1);
      query = query.Where(m => m.ReleaseDate >= yearStart && m.ReleaseDate < yearEnd);
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
      .Select(m => new MovieListItem(m, (decimal)(m.Reviews.Average(r => (double?)r.Score) ?? 0)))
      .ToListAsync(cancellationToken);

    return (movies, pagination);
  }

  public async Task<Movie?> GetMovieAsync(Guid id, bool includePeople, CancellationToken cancellationToken)
  {
    return await GetMovieInternalAsync(id, includePeople, false, cancellationToken);
  }

  public async Task<Movie?> GetMovieReadOnlyAsync(Guid id, bool includePeople, CancellationToken cancellationToken)
  {
    return await GetMovieInternalAsync(id, includePeople, true, cancellationToken);
  }

  private async Task<Movie?> GetMovieInternalAsync(Guid id, bool includePeople, bool readOnly, CancellationToken cancellationToken)
  {
    var query = Context.Movies
      .Include(m => m.Details)
      .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
      .Include(m => m.Reviews)
      .AsSplitQuery()
      .AsQueryable();

    if (readOnly)
    {
      query = query.AsNoTracking();
    }

    if (includePeople)
    {
      query = query.Include(m => m.CastCrews).ThenInclude(cc => cc.Person);
    }

    return await query.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
  }
}
