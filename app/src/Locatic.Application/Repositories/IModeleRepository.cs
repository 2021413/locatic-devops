using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Repositories;

public interface IModeleRepository : IRepository<Modele>
{
    /// <summary>Charge les modèles avec leur marque (Modele → Marque).</summary>
    Task<IReadOnlyList<Modele>> GetAllWithMarqueAsync(CancellationToken cancellationToken = default);
}
