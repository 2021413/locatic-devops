using Locatic.Application.Repositories;
using Locatic.Domain.Entities;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Repositories;

public class MarqueRepository : RepositoryBase<Marque>, IMarqueRepository
{
    public MarqueRepository(LocaticDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Marque>> GetAllWithModelesAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(m => m.Modeles)
            .OrderBy(m => m.Nom)
            .ToListAsync(cancellationToken);

    public async Task<bool> NomExisteAsync(string nom, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(m => m.Nom == nom, cancellationToken);
}
