using Locatic.Application.Common;
using Locatic.Application.Repositories;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public class VoitureService : IVoitureService
{
    private readonly IVoitureRepository _voitures;
    private readonly IModeleRepository _modeles;
    private readonly IReservationRepository _reservations;

    public VoitureService(
        IVoitureRepository voitures,
        IModeleRepository modeles,
        IReservationRepository reservations)
    {
        _voitures = voitures;
        _modeles = modeles;
        _reservations = reservations;
    }

    public Task<IReadOnlyList<Voiture>> ListerAsync(CancellationToken cancellationToken = default)
        => _voitures.GetAllWithDetailsAsync(cancellationToken);

    public Task<Voiture?> ObtenirDetailAsync(int id, CancellationToken cancellationToken = default)
        => _voitures.GetByIdWithDetailsAsync(id, cancellationToken);

    public Task<Voiture?> ObtenirAsync(int id, CancellationToken cancellationToken = default)
        => _voitures.GetByIdAsync(id, cancellationToken);

    public async Task<OperationResult> CreerAsync(Voiture voiture, CancellationToken cancellationToken = default)
    {
        var validation = await ValiderAsync(voiture, cancellationToken);
        if (!validation.Succes)
            return validation;

        voiture.Immatriculation = NormaliserImmatriculation(voiture.Immatriculation);
        await _voitures.AddAsync(voiture, cancellationToken);
        return OperationResult.Reussi();
    }

    public async Task<OperationResult> ModifierAsync(Voiture voiture, CancellationToken cancellationToken = default)
    {
        var existante = await _voitures.GetByIdAsync(voiture.Id, cancellationToken);
        if (existante is null)
            return OperationResult.Echec("Voiture introuvable.");

        var validation = await ValiderAsync(voiture, cancellationToken);
        if (!validation.Succes)
            return validation;

        existante.Immatriculation = NormaliserImmatriculation(voiture.Immatriculation);
        existante.Annee = voiture.Annee;
        existante.TarifJournalier = voiture.TarifJournalier;
        existante.NombrePlaces = voiture.NombrePlaces;
        existante.Carburant = voiture.Carburant;
        existante.ModeleId = voiture.ModeleId;

        await _voitures.UpdateAsync(existante, cancellationToken);
        return OperationResult.Reussi();
    }

    public async Task<OperationResult> SupprimerAsync(int id, CancellationToken cancellationToken = default)
    {
        var voiture = await _voitures.GetByIdAsync(id, cancellationToken);
        if (voiture is null)
            return OperationResult.Echec("Voiture introuvable.");

        // Règle d'intégrité : on n'efface pas une voiture liée à des réservations.
        if (await _reservations.VoitureADesReservationsAsync(id, cancellationToken))
            return OperationResult.Echec("Impossible de supprimer cette voiture : elle est liée à des réservations.");

        await _voitures.DeleteAsync(voiture, cancellationToken);
        return OperationResult.Reussi();
    }

    private async Task<OperationResult> ValiderAsync(Voiture voiture, CancellationToken cancellationToken)
    {
        var immat = NormaliserImmatriculation(voiture.Immatriculation);
        if (string.IsNullOrWhiteSpace(immat))
            return OperationResult.Echec("L'immatriculation est obligatoire.");

        if (!await _modeles.ExistsAsync(voiture.ModeleId, cancellationToken))
            return OperationResult.Echec("Le modèle sélectionné est introuvable.");

        if (await _voitures.ImmatriculationExisteAsync(immat, voiture.Id == 0 ? null : voiture.Id, cancellationToken))
            return OperationResult.Echec($"L'immatriculation « {immat} » est déjà utilisée.");

        if (voiture.TarifJournalier <= 0)
            return OperationResult.Echec("Le tarif journalier doit être strictement positif.");

        return OperationResult.Reussi();
    }

    private static string NormaliserImmatriculation(string? valeur)
        => (valeur ?? string.Empty).Trim().ToUpperInvariant();
}
