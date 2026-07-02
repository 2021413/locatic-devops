using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Repositories;

public interface IMarqueRepository : IRepository<Marque>
{
    /// <summary>Charge les marques avec leurs modèles (pour l'affichage groupé).</summary>
    Task<IReadOnlyList<Marque>> GetAllWithModelesAsync(CancellationToken cancellationToken = default);

    Task<bool> NomExisteAsync(string nom, CancellationToken cancellationToken = default);
}
