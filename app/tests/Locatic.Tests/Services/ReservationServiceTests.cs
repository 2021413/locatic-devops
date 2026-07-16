using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Locatic.Tests.Services;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreerAsync_Refuse_SiDateFinAnterieureOuEgale()
    {
        await using var ctx = await SqliteTestContext.CreateAsync();
        using var scope = ctx.Provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReservationService>();

        var result = await service.CreerAsync(new Reservation
        {
            ClientId = 1,
            VoitureId = 2,
            DateDebut = new DateOnly(2026, 8, 10),
            DateFin = new DateOnly(2026, 8, 10)
        });

        Assert.False(result.Succes);
        Assert.Contains("postérieure", result.Erreur, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreerAsync_Refuse_SiChevauchement()
    {
        await using var ctx = await SqliteTestContext.CreateAsync();
        using var scope = ctx.Provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReservationService>();

        // Seed : voiture 1 déjà réservée du 2026-07-01 au 2026-07-05.
        var result = await service.CreerAsync(new Reservation
        {
            ClientId = 2,
            VoitureId = 1,
            DateDebut = new DateOnly(2026, 7, 3),
            DateFin = new DateOnly(2026, 7, 8)
        });

        Assert.False(result.Succes);
        Assert.Contains("déjà réservée", result.Erreur, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreerAsync_CalculeMontant_EtReussit()
    {
        await using var ctx = await SqliteTestContext.CreateAsync();
        using var scope = ctx.Provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReservationService>();

        // Voiture 2 : tarif 45.00 €/jour, 3 jours.
        var result = await service.CreerAsync(new Reservation
        {
            ClientId = 1,
            VoitureId = 2,
            DateDebut = new DateOnly(2026, 9, 1),
            DateFin = new DateOnly(2026, 9, 4)
        });

        Assert.True(result.Succes);
        Assert.NotNull(result.Valeur);
        Assert.Equal(135.00m, result.Valeur!.Montant);
    }
}
