using Locatic.Infrastructure;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Locatic.Tests.Infrastructure;

internal static class SqliteServiceProviderFactory
{
    public static async Task<(ServiceProvider Provider, string DbPath)> CreateAsync()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"locatic-svc-{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocatic($"Data Source={dbPath}");

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocaticDbContext>();
        await context.Database.MigrateAsync();

        return (provider, dbPath);
    }

    public static void Cleanup(ServiceProvider provider, string dbPath)
    {
        provider.Dispose();
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }
}
