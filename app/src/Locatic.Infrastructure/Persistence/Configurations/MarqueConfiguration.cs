using Locatic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locatic.Infrastructure.Persistence.Configurations;

public class MarqueConfiguration : IEntityTypeConfiguration<Marque>
{
    public void Configure(EntityTypeBuilder<Marque> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.PaysOrigine)
            .HasMaxLength(80);

        builder.HasIndex(m => m.Nom).IsUnique();

        // Une marque regroupe plusieurs modèles ; supprimer la marque supprime ses modèles.
        builder.HasMany(m => m.Modeles)
            .WithOne(mo => mo.Marque!)
            .HasForeignKey(mo => mo.MarqueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new Marque { Id = 1, Nom = "Renault", PaysOrigine = "France" },
            new Marque { Id = 2, Nom = "Peugeot", PaysOrigine = "France" },
            new Marque { Id = 3, Nom = "BMW", PaysOrigine = "Allemagne" });
    }
}
