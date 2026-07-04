namespace MovieAPI.Infrastructure.Interfaces;

public interface IRepositoryBase<TEntity>
{
  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
  Task<IList<Guid>> GetMissingIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken);
  Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
  Task AddAsync(TEntity entity, CancellationToken cancellationToken);
  void Delete(TEntity entity);
}
