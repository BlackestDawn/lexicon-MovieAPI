using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public class PersonRepository(AppDbContext context) : RepositoryBase<Person>(context), IPersonRepository
{
  protected override DbSet<Person> Set => Context.Persons;

  public Task<(IEnumerable<Person>, PaginationMetadata?)> GetPersonsAsync(PersonSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return GetPersonsInternalAsync(searchParams, page, pageSize, false, cancellationToken);
  }

  public Task<(IEnumerable<Person>, PaginationMetadata?)> GetPersonsReadOnlyAsync(PersonSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    return GetPersonsInternalAsync(searchParams, page, pageSize, true, cancellationToken);
  }

  private async Task<(IEnumerable<Person>, PaginationMetadata?)> GetPersonsInternalAsync(PersonSearchParams searchParams, int page, int pageSize, bool readOnly, CancellationToken cancellationToken)
  {
    var query = Context.Persons.AsQueryable();

    if (readOnly)
    {
      query = query.AsNoTracking();
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Name))
    {
      query = query.Where(p => p.GivenName.Contains(searchParams.Name) || p.LastName.Contains(searchParams.Name));
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Genre))
    {
      query = query.Where(p => p.CastCrews.Any(cc => cc.Movie.MovieGenres.Any(mg => mg.Genre.Slug == searchParams.Genre || mg.Genre.Name == searchParams.Genre)));
    }

    if (searchParams.Year.HasValue)
    {
      var yearStart = new DateOnly(searchParams.Year.Value, 1, 1);
      var yearEnd = yearStart.AddYears(1);
      query = query.Where(p => p.CastCrews.Any(cc => cc.Movie.ReleaseDate >= yearStart && cc.Movie.ReleaseDate < yearEnd));
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var pagination = new PaginationMetadata(totalCount, pageSize, page);

    var persons = await query
      .OrderBy(p => p.LastName).ThenBy(p => p.GivenName)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (persons, pagination);
  }

  public Task<Person?> GetPersonAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    return GetPersonInternalAsync(id, includeMovies, false, cancellationToken);
  }

  public Task<Person?> GetPersonReadOnlyAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    return GetPersonInternalAsync(id, includeMovies, true, cancellationToken);
  }

  private async Task<Person?> GetPersonInternalAsync(Guid id, bool includeMovies, bool readOnly, CancellationToken cancellationToken)
  {
    var query = Context.Persons.AsQueryable();

    if (readOnly)
    {
      query = query.AsNoTracking();
    }

    if (includeMovies)
    {
      query = query.Include(p => p.CastCrews).ThenInclude(cc => cc.Movie);
    }

    return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }
}
