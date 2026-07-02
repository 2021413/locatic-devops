using Locatic.Infrastructure;
using Locatic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Persistance SQLite + repositories + services applicatifs (injection de dépendances).
var connectionString = builder.Configuration.GetConnectionString("LocaticDb")
    ?? "Data Source=locatic.db";
builder.Services.AddLocatic(connectionString);

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
