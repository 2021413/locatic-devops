using Locatic.Application.Common;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Repositories;

/// <summary>
/// Implémentation EF Core générique du contrat <see cref="IRepository{T}"/>.
/// Chaque opération de modification persiste immédiatement les changements.
/// </summary>
public class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly LocaticDbContext Context;
    protected readonly DbSet<T> Set;

    public RepositoryBase(LocaticDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new object?[] { id }, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Set.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new object?[] { id }, cancellationToken) is not null;
}
