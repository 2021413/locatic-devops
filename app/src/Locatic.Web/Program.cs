using Locatic.Infrastructure;
using Locatic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Derrière Nginx / minikube : accepter les en-têtes du reverse proxy local.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllersWithViews();

// Persistance SQLite + repositories + services applicatifs (injection de dépendances).
// Surcharge possible via ConnectionStrings__LocaticDb (conteneur : /data/locatic.db).
var connectionString = builder.Configuration.GetConnectionString("LocaticDb")
    ?? "Data Source=locatic.db";
EnsureSqliteDirectoryExists(connectionString);
builder.Services.AddLocatic(connectionString);

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<LocaticDbContext>("sqlite", tags: ["ready"]);

var app = builder.Build();

app.UseForwardedHeaders();

// Applique les migrations au démarrage : la base est créée si besoin et reste à jour.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LocaticDbContext>();
    context.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// En conteneur / derrière Nginx, le proxy termine le HTTP : pas de redirection HTTPS.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseHttpMetrics();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapMetrics();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void EnsureSqliteDirectoryExists(string connectionString)
{
    const string prefix = "Data Source=";
    var start = connectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
    if (start < 0)
    {
        return;
    }

    var path = connectionString[(start + prefix.Length)..].Trim().Trim('"');
    var separator = path.IndexOf(';');
    if (separator >= 0)
    {
        path = path[..separator];
    }

    var directory = Path.GetDirectoryName(path);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

// Exposé pour les tests d'intégration (WebApplicationFactory).
public partial class Program;
