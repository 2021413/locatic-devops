using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public interface IReservationService
{
    Task<IReadOnlyList<Reservation>> ListerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée une réservation après application des règles métier
    /// (cohérence des dates, disponibilité de la voiture) et calcul du montant.
    /// </summary>
    Task<OperationResult<Reservation>> CreerAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
