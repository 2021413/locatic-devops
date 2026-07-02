using Locatic.Application.Repositories;
using Locatic.Application.Services;
using Locatic.Infrastructure.Persistence;
using Locatic.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Locatic.Infrastructure;

/// <summary>
/// Point d'entrée d'enregistrement des dépendances de l'accès aux données
/// et de la logique applicative. Garde le Program.cs du Web minimal.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddLocatic(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LocaticDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories (accès aux données)
        services.AddScoped<IMarqueRepository, MarqueRepository>();
        services.AddScoped<IModeleRepository, ModeleRepository>();
        services.AddScoped<IVoitureRepository, VoitureRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        // Services (logique applicative)
        services.AddScoped<IMarqueService, MarqueService>();
        services.AddScoped<IModeleService, ModeleService>();
        services.AddScoped<IVoitureService, VoitureService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ITableauDeBordService, TableauDeBordService>();

        return services;
    }
}
