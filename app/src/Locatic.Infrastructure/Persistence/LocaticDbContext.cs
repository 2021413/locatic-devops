using Locatic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core de l'application : point d'entrée unique vers la base SQLite.
/// </summary>
public class LocaticDbContext : DbContext
{
    public LocaticDbContext(DbContextOptions<LocaticDbContext> options) : base(options)
    {
    }

    public DbSet<Marque> Marques => Set<Marque>();
    public DbSet<Modele> Modeles => Set<Modele>();
    public DbSet<Voiture> Voitures => Set<Voiture>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applique toutes les classes IEntityTypeConfiguration de cet assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocaticDbContext).Assembly);
    }
}
