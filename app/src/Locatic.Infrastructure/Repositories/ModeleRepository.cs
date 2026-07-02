using Locatic.Application.Repositories;
using Locatic.Domain.Entities;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Repositories;

public class ModeleRepository : RepositoryBase<Modele>, IModeleRepository
{
    public ModeleRepository(LocaticDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Modele>> GetAllWithMarqueAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(m => m.Marque)
            .OrderBy(m => m.Marque!.Nom)
            .ThenBy(m => m.Nom)
            .ToListAsync(cancellationToken);
}
