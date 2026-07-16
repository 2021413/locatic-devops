using Locatic.Application.Repositories;
using Locatic.Application.Services;
using Locatic.Infrastructure.Persistence;
using Locatic.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Locatic.Tests.Infrastructure;

internal sealed class SqliteTestContext : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public ServiceProvider Provider { get; }

    private SqliteTestContext(SqliteConnection connection, ServiceProvider provider)
    {
        _connection = connection;
        Provider = provider;
    }

    public static async Task<SqliteTestContext> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<LocaticDbContext>(options => options.UseSqlite(connection));

        services.AddScoped<IMarqueRepository, MarqueRepository>();
        services.AddScoped<IModeleRepository, ModeleRepository>();
        services.AddScoped<IVoitureRepository, VoitureRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.AddScoped<IMarqueService, MarqueService>();
        services.AddScoped<IModeleService, ModeleService>();
        services.AddScoped<IVoitureService, VoitureService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ITableauDeBordService, TableauDeBordService>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocaticDbContext>();
        await context.Database.MigrateAsync();

        return new SqliteTestContext(connection, provider);
    }

    public async ValueTask DisposeAsync()
    {
        await Provider.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
