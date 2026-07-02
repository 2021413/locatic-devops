namespace Locatic.Domain.Entities;

/// <summary>
/// Un modèle de voiture (Clio, 208, Série 3...), rattaché à une seule marque.
/// Un modèle se décline en plusieurs voitures.
/// </summary>
public class Modele
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    /// <summary>Clé étrangère vers la marque parente.</summary>
    public int MarqueId { get; set; }

    /// <summary>Propriété de navigation : la marque qui produit ce modèle.</summary>
    public Marque? Marque { get; set; }

    /// <summary>Propriété de navigation : les voitures concrètes de ce modèle.</summary>
    public ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();
}
