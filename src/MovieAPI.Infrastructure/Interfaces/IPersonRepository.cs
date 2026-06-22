using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IPersonRepository : IRepositoryBase<Person>
{
  Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleAsync(PeopleSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<(IEnumerable<Person>, PaginationMetadata?)> GetPeopleReadOnlyAsync(PeopleSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<Person?> GetPersonAsync(Guid id, bool includeMovies, CancellationToken cancellationToken);
}
