using Locatic.Application.Common;
using Locatic.Application.Repositories;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IVoitureRepository _voitures;
    private readonly IClientRepository _clients;

    public ReservationService(
        IReservationRepository reservations,
        IVoitureRepository voitures,
        IClientRepository clients)
    {
        _reservations = reservations;
        _voitures = voitures;
        _clients = clients;
    }

    public Task<IReadOnlyList<Reservation>> ListerAsync(CancellationToken cancellationToken = default)
        => _reservations.GetAllWithDetailsAsync(cancellationToken);

    public async Task<OperationResult<Reservation>> CreerAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        // Règle métier n°1 : la date de fin ne peut être antérieure (ou égale) à la date de début.
        if (reservation.DateFin <= reservation.DateDebut)
            return OperationResult<Reservation>.Echec("La date de fin doit être postérieure à la date de début.");

        var client = await _clients.GetByIdAsync(reservation.ClientId, cancellationToken);
        if (client is null)
            return OperationResult<Reservation>.Echec("Le client sélectionné est introuvable.");

        var voiture = await _voitures.GetByIdAsync(reservation.VoitureId, cancellationToken);
        if (voiture is null)
            return OperationResult<Reservation>.Echec("La voiture sélectionnée est introuvable.");

        // Règle métier n°2 : la voiture ne doit pas déjà être réservée sur la période.
        var chevauche = await _reservations.ChevauchementExisteAsync(
            reservation.VoitureId, reservation.DateDebut, reservation.DateFin, null, cancellationToken);
        if (chevauche)
            return OperationResult<Reservation>.Echec("Cette voiture est déjà réservée sur la période demandée.");

        // Calcul automatique du montant : tarif journalier × nombre de jours.
        var nombreJours = Math.Max(1, reservation.DateFin.DayNumber - reservation.DateDebut.DayNumber);
        reservation.Montant = voiture.TarifJournalier * nombreJours;

        await _reservations.AddAsync(reservation, cancellationToken);
        return OperationResult<Reservation>.Reussi(reservation);
    }
}
