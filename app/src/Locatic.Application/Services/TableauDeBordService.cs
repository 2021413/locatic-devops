using Locatic.Application.Repositories;

namespace Locatic.Application.Services;

public class TableauDeBordService : ITableauDeBordService
{
    private readonly IVoitureRepository _voitures;
    private readonly IClientRepository _clients;
    private readonly IReservationRepository _reservations;
    private readonly IMarqueRepository _marques;
    private readonly IModeleRepository _modeles;

    public TableauDeBordService(
        IVoitureRepository voitures,
        IClientRepository clients,
        IReservationRepository reservations,
        IMarqueRepository marques,
        IModeleRepository modeles)
    {
        _voitures = voitures;
        _clients = clients;
        _reservations = reservations;
        _marques = marques;
        _modeles = modeles;
    }

    public async Task<TableauDeBordStats> ObtenirStatsAsync(DateOnly aujourdhui, CancellationToken cancellationToken = default)
    {
        var voitures = await _voitures.GetAllAsync(cancellationToken);
        var clients = await _clients.GetAllAsync(cancellationToken);
        var reservations = await _reservations.GetAllAsync(cancellationToken);
        var marques = await _marques.GetAllAsync(cancellationToken);
        var modeles = await _modeles.GetAllAsync(cancellationToken);
        var enCours = await _reservations.CompterEnCoursAsync(aujourdhui, cancellationToken);

        return new TableauDeBordStats(
            NombreVoitures: voitures.Count,
            NombreClients: clients.Count,
            NombreReservations: reservations.Count,
            ReservationsEnCours: enCours,
            NombreMarques: marques.Count,
            NombreModeles: modeles.Count);
    }
}
