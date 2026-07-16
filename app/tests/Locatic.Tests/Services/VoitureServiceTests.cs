using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Domain.Enums;
using Locatic.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Locatic.Tests.Services;

public class VoitureServiceTests
{
    [Fact]
    public async Task CreerAsync_Refuse_ImmatriculationDejaUtilisee()
    {
        var (provider, dbPath) = await SqliteServiceProviderFactory.CreateAsync();
        try
        {
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IVoitureService>();

            var result = await service.CreerAsync(new Voiture
            {
                Immatriculation = "aa-123-bb",
                Annee = 2024,
                TarifJournalier = 40m,
                NombrePlaces = 5,
                Carburant = TypeCarburant.Essence,
                ModeleId = 1
            });

            Assert.False(result.Succes);
            Assert.Contains("déjà utilisée", result.Erreur, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            SqliteServiceProviderFactory.Cleanup(provider, dbPath);
        }
    }

    [Fact]
    public async Task SupprimerAsync_Refuse_SiLieeAReservations()
    {
        var (provider, dbPath) = await SqliteServiceProviderFactory.CreateAsync();
        try
        {
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IVoitureService>();

            // Voiture 1 liée à la réservation seed.
            var result = await service.SupprimerAsync(1);

            Assert.False(result.Succes);
            Assert.Contains("réservations", result.Erreur, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            SqliteServiceProviderFactory.Cleanup(provider, dbPath);
        }
    }

    [Fact]
    public async Task CreerAsync_Reussit_AvecImmatriculationUnique()
    {
        var (provider, dbPath) = await SqliteServiceProviderFactory.CreateAsync();
        try
        {
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IVoitureService>();

            var result = await service.CreerAsync(new Voiture
            {
                Immatriculation = "zz-999-yy",
                Annee = 2025,
                TarifJournalier = 55m,
                NombrePlaces = 5,
                Carburant = TypeCarburant.Diesel,
                ModeleId = 2
            });

            Assert.True(result.Succes);
        }
        finally
        {
            SqliteServiceProviderFactory.Cleanup(provider, dbPath);
        }
    }
}
