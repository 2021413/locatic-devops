namespace Locatic.Application.Services;

/// <summary>Indicateurs agrégés affichés sur le tableau de bord.</summary>
public record TableauDeBordStats(
    int NombreVoitures,
    int NombreClients,
    int NombreReservations,
    int ReservationsEnCours,
    int NombreMarques,
    int NombreModeles);
