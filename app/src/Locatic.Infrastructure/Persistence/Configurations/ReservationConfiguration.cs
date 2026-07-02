using Locatic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locatic.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Montant)
            .HasColumnType("decimal(10,2)");

        // Le nombre de jours est dérivé des dates : non persisté.
        builder.Ignore(r => r.NombreJours);

        builder.HasOne(r => r.Voiture)
            .WithMany(v => v.Reservations)
            .HasForeignKey(r => r.VoitureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Reservation
            {
                Id = 1,
                ClientId = 1,
                VoitureId = 1,
                DateDebut = new DateOnly(2026, 7, 1),
                DateFin = new DateOnly(2026, 7, 5),
                Montant = 159.60m
            });
    }
}
