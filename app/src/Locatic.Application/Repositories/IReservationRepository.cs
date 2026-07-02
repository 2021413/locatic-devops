using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
    /// <summary>Charge les réservations avec le client, la voiture, son modèle et sa marque.</summary>
    Task<IReadOnlyList<Reservation>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indique si la voiture est déjà réservée sur une période qui chevauche [debut, fin].
    /// </summary>
    Task<bool> ChevauchementExisteAsync(
        int voitureId,
        DateOnly debut,
        DateOnly fin,
        int? exclureReservationId = null,
        CancellationToken cancellationToken = default);

    Task<int> CompterEnCoursAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>Indique si une voiture est référencée par au moins une réservation.</summary>
    Task<bool> VoitureADesReservationsAsync(int voitureId, CancellationToken cancellationToken = default);
}
