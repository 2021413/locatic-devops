using Locatic.Application.Repositories;
using Locatic.Domain.Entities;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Repositories;

public class VoitureRepository : RepositoryBase<Voiture>, IVoitureRepository
{
    public VoitureRepository(LocaticDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Voiture>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(v => v.Modele!)
                .ThenInclude(m => m.Marque)
            .OrderBy(v => v.Modele!.Marque!.Nom)
            .ThenBy(v => v.Modele!.Nom)
            .ToListAsync(cancellationToken);

    public async Task<Voiture?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(v => v.Modele!)
                .ThenInclude(m => m.Marque)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<bool> ImmatriculationExisteAsync(string immatriculation, int? exclureId = null, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(
            v => v.Immatriculation == immatriculation && (exclureId == null || v.Id != exclureId),
            cancellationToken);
}
