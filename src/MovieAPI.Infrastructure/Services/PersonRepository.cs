using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public class PersonRepository(AppDbContext context) : RepositoryBase<Person>(context), IPersonRepository
{
  protected override DbSet<Person> Set => Context.Persons;

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
    var query = Context.Persons.AsQueryable();

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
    return await GetPersonInternalAsync(id, includeMovies, false, cancellationToken);
  }

  public async Task<Person?> GetPersonReadOnlyAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    return await GetPersonInternalAsync(id, includeMovies, true, cancellationToken);
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
