using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Repositories;

public interface IVoitureRepository : IRepository<Voiture>
{
    /// <summary>Charge les voitures avec leur modèle et leur marque (Voiture → Modele → Marque).</summary>
    Task<IReadOnlyList<Voiture>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>Charge une voiture avec son modèle et sa marque.</summary>
    Task<Voiture?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Vérifie l'unicité d'une immatriculation (en excluant éventuellement une voiture en cours d'édition).</summary>
    Task<bool> ImmatriculationExisteAsync(string immatriculation, int? exclureId = null, CancellationToken cancellationToken = default);
}
