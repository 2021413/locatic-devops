namespace Locatic.Domain.Entities;

/// <summary>
/// Un constructeur automobile (Renault, Peugeot, BMW...).
/// Une marque regroupe plusieurs modèles.
/// </summary>
public class Marque
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    /// <summary>Pays d'origine du constructeur (optionnel).</summary>
    public string? PaysOrigine { get; set; }

    /// <summary>Propriété de navigation : les modèles déclinés par cette marque.</summary>
    public ICollection<Modele> Modeles { get; set; } = new List<Modele>();
}
