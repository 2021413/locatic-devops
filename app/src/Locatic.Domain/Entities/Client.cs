namespace Locatic.Domain.Entities;

/// <summary>
/// Une personne qui loue des voitures.
/// Un client peut poser plusieurs réservations.
/// </summary>
public class Client
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string Prenom { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telephone { get; set; }

    /// <summary>Propriété de navigation : les réservations du client.</summary>
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>Nom complet, pratique pour l'affichage.</summary>
    public string NomComplet => $"{Prenom} {Nom}".Trim();
}
