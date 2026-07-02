using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public interface IModeleService
{
    Task<IReadOnlyList<Modele>> ListerAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreerAsync(Modele modele, CancellationToken cancellationToken = default);
}
