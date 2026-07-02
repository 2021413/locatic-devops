namespace Locatic.Application.Common;

/// <summary>
/// Résultat d'une opération métier : succès, ou échec avec un message clair.
/// Permet de remonter une règle métier violée jusqu'au controller
/// sans recourir à des exceptions de contrôle de flux.
/// </summary>
public class OperationResult
{
    public bool Succes { get; }
    public string? Erreur { get; }

    protected OperationResult(bool succes, string? erreur)
    {
        Succes = succes;
        Erreur = erreur;
    }

    public static OperationResult Reussi() => new(true, null);
    public static OperationResult Echec(string erreur) => new(false, erreur);
}

/// <summary>Résultat d'une opération métier qui produit une valeur en cas de succès.</summary>
public class OperationResult<T> : OperationResult
{
    public T? Valeur { get; }

    private OperationResult(bool succes, string? erreur, T? valeur)
        : base(succes, erreur)
    {
        Valeur = valeur;
    }

    public static OperationResult<T> Reussi(T valeur) => new(true, null, valeur);
    public static new OperationResult<T> Echec(string erreur) => new(false, erreur, default);
}
