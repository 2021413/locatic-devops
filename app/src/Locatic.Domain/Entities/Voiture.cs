using Locatic.Domain.Enums;

namespace Locatic.Domain.Entities;

/// <summary>
/// Un véhicule concret du parc, rattaché à un seul modèle.
/// Depuis une voiture on remonte à son modèle puis à sa marque
/// (Voiture → Modele → Marque).
/// </summary>
public class Voiture
{
    public int Id { get; set; }

    /// <summary>Plaque d'immatriculation (unique dans le parc).</summary>
    public string Immatriculation { get; set; } = string.Empty;

    public int Annee { get; set; }

    /// <summary>Tarif de location par jour, en euros.</summary>
    public decimal TarifJournalier { get; set; }

    public int NombrePlaces { get; set; }

    public TypeCarburant Carburant { get; set; }

    /// <summary>Clé étrangère vers le modèle.</summary>
    public int ModeleId { get; set; }

    /// <summary>Propriété de navigation : le modèle de la voiture.</summary>
    public Modele? Modele { get; set; }

    /// <summary>Propriété de navigation : les réservations posées sur cette voiture.</summary>
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
