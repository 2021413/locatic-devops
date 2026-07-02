using Locatic.Application.Common;
using Locatic.Application.Repositories;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public class MarqueService : IMarqueService
{
    private readonly IMarqueRepository _marques;

    public MarqueService(IMarqueRepository marques)
    {
        _marques = marques;
    }

    public Task<IReadOnlyList<Marque>> ListerAsync(CancellationToken cancellationToken = default)
        => _marques.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<Marque>> ListerAvecModelesAsync(CancellationToken cancellationToken = default)
        => _marques.GetAllWithModelesAsync(cancellationToken);

    public async Task<OperationResult> CreerAsync(Marque marque, CancellationToken cancellationToken = default)
    {
        var nom = (marque.Nom ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nom))
            return OperationResult.Echec("Le nom de la marque est obligatoire.");

        if (await _marques.NomExisteAsync(nom, cancellationToken))
            return OperationResult.Echec($"La marque « {nom} » existe déjà.");

        marque.Nom = nom;
        marque.PaysOrigine = string.IsNullOrWhiteSpace(marque.PaysOrigine) ? null : marque.PaysOrigine.Trim();
        await _marques.AddAsync(marque, cancellationToken);
        return OperationResult.Reussi();
    }
}
