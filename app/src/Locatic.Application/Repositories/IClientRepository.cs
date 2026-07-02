using Locatic.Application.Common;
using Locatic.Domain.Entities;

namespace Locatic.Application.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task<bool> EmailExisteAsync(string email, int? exclureId = null, CancellationToken cancellationToken = default);
}
