using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public interface IClientService
{
    Task<IReadOnlyList<Client>> ListerAsync(CancellationToken cancellationToken = default);
    Task<Client?> ObtenirAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreerAsync(Client client, CancellationToken cancellationToken = default);
}
