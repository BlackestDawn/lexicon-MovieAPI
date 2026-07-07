using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Interfaces;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Infrastructure.Services;

public abstract class RepositoryBase<TEntity>(AppDbContext context) : IRepositoryBase<TEntity>
  where TEntity : class, IEntity
{
  protected AppDbContext Context { get; } = context;

  protected abstract DbSet<TEntity> Set { get; }

  public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
  {
    return Set.AnyAsync(e => e.Id == id, cancellationToken);
  }

  public async Task<IList<Guid>> GetMissingIdsAsync(ICollection<Guid> ids, CancellationToken cancellationToken)
  {
    var found = await Set
      .Where(e => ids.Contains(e.Id))
      .Select(e => e.Id)
      .ToListAsync(cancellationToken);
    return [..ids.Except(found)];
  }

  public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return await Context.SaveChangesAsync(cancellationToken) > 0;
  }

  public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
  {
    await Set.AddAsync(entity, cancellationToken);
  }

  public void Delete(TEntity entity)
  {
    Set.Remove(entity);
  }
}
