using Locatic.Application.Repositories;
using Locatic.Domain.Entities;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Repositories;

public class ClientRepository : RepositoryBase<Client>, IClientRepository
{
    public ClientRepository(LocaticDbContext context) : base(context)
    {
    }

    public override async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .OrderBy(c => c.Nom)
            .ThenBy(c => c.Prenom)
            .ToListAsync(cancellationToken);

    public async Task<bool> EmailExisteAsync(string email, int? exclureId = null, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(
            c => c.Email == email && (exclureId == null || c.Id != exclureId),
            cancellationToken);
}
