using Locatic.Application.Repositories;
using Locatic.Domain.Entities;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Repositories;

public class ReservationRepository : RepositoryBase<Reservation>, IReservationRepository
{
    public ReservationRepository(LocaticDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Reservation>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(r => r.Client)
            .Include(r => r.Voiture!)
                .ThenInclude(v => v.Modele!)
                    .ThenInclude(m => m.Marque)
            .OrderByDescending(r => r.DateDebut)
            .ToListAsync(cancellationToken);

    public async Task<bool> ChevauchementExisteAsync(
        int voitureId,
        DateOnly debut,
        DateOnly fin,
        int? exclureReservationId = null,
        CancellationToken cancellationToken = default)
        => await Set.AnyAsync(
            r => r.VoitureId == voitureId
                 && (exclureReservationId == null || r.Id != exclureReservationId)
                 // Deux périodes se chevauchent si chacune commence avant la fin de l'autre.
                 && debut < r.DateFin
                 && r.DateDebut < fin,
            cancellationToken);

    public async Task<int> CompterEnCoursAsync(DateOnly date, CancellationToken cancellationToken = default)
        => await Set.CountAsync(r => r.DateDebut <= date && date < r.DateFin, cancellationToken);

    public async Task<bool> VoitureADesReservationsAsync(int voitureId, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(r => r.VoitureId == voitureId, cancellationToken);
}
