using Locatic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locatic.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nom)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(c => c.Prenom)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Telephone)
            .HasMaxLength(20);

        builder.HasIndex(c => c.Email).IsUnique();

        // Le nom complet est calculé : EF ne doit pas tenter de le mapper.
        builder.Ignore(c => c.NomComplet);

        builder.HasMany(c => c.Reservations)
            .WithOne(r => r.Client!)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Client { Id = 1, Nom = "Martin", Prenom = "Lucas", Email = "lucas.martin@example.com", Telephone = "0601020304" },
            new Client { Id = 2, Nom = "Bernard", Prenom = "Emma", Email = "emma.bernard@example.com", Telephone = "0605060708" });
    }
}
