using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public interface IVoitureService
{
    Task<IReadOnlyList<Voiture>> ListerAsync(CancellationToken cancellationToken = default);
    Task<Voiture?> ObtenirDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<Voiture?> ObtenirAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreerAsync(Voiture voiture, CancellationToken cancellationToken = default);
    Task<OperationResult> ModifierAsync(Voiture voiture, CancellationToken cancellationToken = default);
    Task<OperationResult> SupprimerAsync(int id, CancellationToken cancellationToken = default);
}
