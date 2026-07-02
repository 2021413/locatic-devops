using Locatic.Application.Common;
using Locatic.Application.Repositories;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public class ModeleService : IModeleService
{
    private readonly IModeleRepository _modeles;
    private readonly IMarqueRepository _marques;

    public ModeleService(IModeleRepository modeles, IMarqueRepository marques)
    {
        _modeles = modeles;
        _marques = marques;
    }

    public Task<IReadOnlyList<Modele>> ListerAsync(CancellationToken cancellationToken = default)
        => _modeles.GetAllWithMarqueAsync(cancellationToken);

    public async Task<OperationResult> CreerAsync(Modele modele, CancellationToken cancellationToken = default)
    {
        var nom = (modele.Nom ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nom))
            return OperationResult.Echec("Le nom du modèle est obligatoire.");

        // Règle d'intégrité : on ne rattache qu'à une marque qui existe réellement.
        if (!await _marques.ExistsAsync(modele.MarqueId, cancellationToken))
            return OperationResult.Echec("La marque sélectionnée est introuvable.");

        modele.Nom = nom;
        await _modeles.AddAsync(modele, cancellationToken);
        return OperationResult.Reussi();
    }
}
