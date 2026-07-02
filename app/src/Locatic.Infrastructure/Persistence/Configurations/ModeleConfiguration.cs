using Locatic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locatic.Infrastructure.Persistence.Configurations;

public class ModeleConfiguration : IEntityTypeConfiguration<Modele>
{
    public void Configure(EntityTypeBuilder<Modele> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nom)
            .IsRequired()
            .HasMaxLength(100);

        // Un modèle se décline en plusieurs voitures ; on bloque la suppression
        // d'un modèle encore rattaché à des voitures (intégrité gérée côté service).
        builder.HasMany(m => m.Voitures)
            .WithOne(v => v.Modele!)
            .HasForeignKey(v => v.ModeleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Modele { Id = 1, Nom = "Clio", MarqueId = 1 },
            new Modele { Id = 2, Nom = "Captur", MarqueId = 1 },
            new Modele { Id = 3, Nom = "208", MarqueId = 2 },
            new Modele { Id = 4, Nom = "308", MarqueId = 2 },
            new Modele { Id = 5, Nom = "Série 3", MarqueId = 3 },
            new Modele { Id = 6, Nom = "X1", MarqueId = 3 });
    }
}
