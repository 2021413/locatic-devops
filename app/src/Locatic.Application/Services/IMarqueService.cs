using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public interface IMarqueService
{
    Task<IReadOnlyList<Marque>> ListerAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Marque>> ListerAvecModelesAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreerAsync(Marque marque, CancellationToken cancellationToken = default);
}
