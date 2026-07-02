using Locatic.Application.Common;
using Locatic.Application.Repositories;
using Locatic.Domain.Entities;

namespace Locatic.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clients;

    public ClientService(IClientRepository clients)
    {
        _clients = clients;
    }

    public Task<IReadOnlyList<Client>> ListerAsync(CancellationToken cancellationToken = default)
        => _clients.GetAllAsync(cancellationToken);

    public Task<Client?> ObtenirAsync(int id, CancellationToken cancellationToken = default)
        => _clients.GetByIdAsync(id, cancellationToken);

    public async Task<OperationResult> CreerAsync(Client client, CancellationToken cancellationToken = default)
    {
        var email = (client.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(client.Nom) || string.IsNullOrWhiteSpace(client.Prenom))
            return OperationResult.Echec("Le nom et le prénom sont obligatoires.");

        if (await _clients.EmailExisteAsync(email, null, cancellationToken))
            return OperationResult.Echec($"Un client utilise déjà l'email « {email} ».");

        client.Nom = client.Nom.Trim();
        client.Prenom = client.Prenom.Trim();
        client.Email = email;
        client.Telephone = string.IsNullOrWhiteSpace(client.Telephone) ? null : client.Telephone.Trim();
        await _clients.AddAsync(client, cancellationToken);
        return OperationResult.Reussi();
    }
}
