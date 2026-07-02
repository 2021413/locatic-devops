namespace Locatic.Domain.Entities;

/// <summary>
/// La location d'une voiture par un client sur une période donnée.
/// Une réservation concerne une seule voiture et un seul client.
/// </summary>
public class Reservation
{
    public int Id { get; set; }

    public DateOnly DateDebut { get; set; }

    public DateOnly DateFin { get; set; }

    /// <summary>
    /// Montant total de la location, figé au moment de la réservation
    /// (tarif journalier × nombre de jours).
    /// </summary>
    public decimal Montant { get; set; }

    /// <summary>Clé étrangère vers la voiture louée.</summary>
    public int VoitureId { get; set; }

    /// <summary>Propriété de navigation : la voiture louée.</summary>
    public Voiture? Voiture { get; set; }

    /// <summary>Clé étrangère vers le client.</summary>
    public int ClientId { get; set; }

    /// <summary>Propriété de navigation : le client qui loue.</summary>
    public Client? Client { get; set; }

    /// <summary>Nombre de jours facturés (au moins un jour).</summary>
    public int NombreJours => Math.Max(1, DateFin.DayNumber - DateDebut.DayNumber);
}
