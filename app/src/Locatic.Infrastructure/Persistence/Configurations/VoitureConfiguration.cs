using Locatic.Domain.Entities;
using Locatic.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locatic.Infrastructure.Persistence.Configurations;

public class VoitureConfiguration : IEntityTypeConfiguration<Voiture>
{
    public void Configure(EntityTypeBuilder<Voiture> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Immatriculation)
            .IsRequired()
            .HasMaxLength(15);

        builder.HasIndex(v => v.Immatriculation).IsUnique();

        builder.Property(v => v.TarifJournalier)
            .HasColumnType("decimal(10,2)");

        // Stocke l'enum sous forme lisible plutôt qu'un entier opaque.
        builder.Property(v => v.Carburant)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasData(
            new Voiture { Id = 1, Immatriculation = "AA-123-BB", Annee = 2021, TarifJournalier = 39.90m, NombrePlaces = 5, Carburant = TypeCarburant.Essence, ModeleId = 1 },
            new Voiture { Id = 2, Immatriculation = "CD-456-EF", Annee = 2022, TarifJournalier = 45.00m, NombrePlaces = 5, Carburant = TypeCarburant.Diesel, ModeleId = 2 },
            new Voiture { Id = 3, Immatriculation = "GH-789-IJ", Annee = 2023, TarifJournalier = 52.50m, NombrePlaces = 5, Carburant = TypeCarburant.Hybride, ModeleId = 3 },
            new Voiture { Id = 4, Immatriculation = "KL-012-MN", Annee = 2020, TarifJournalier = 75.00m, NombrePlaces = 5, Carburant = TypeCarburant.Essence, ModeleId = 5 },
            new Voiture { Id = 5, Immatriculation = "OP-345-QR", Annee = 2024, TarifJournalier = 89.00m, NombrePlaces = 5, Carburant = TypeCarburant.Electrique, ModeleId = 6 });
    }
}
