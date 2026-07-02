namespace Locatic.Application.Common;

/// <summary>
/// Contrat d'accès aux données générique pour une entité.
/// L'implémentation (EF Core) vit dans la couche Infrastructure :
/// les services ne connaissent que cette abstraction (inversion de dépendance).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
