using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Locatic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PaysOrigine = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modeles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MarqueId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modeles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modeles_Marques_MarqueId",
                        column: x => x.MarqueId,
                        principalTable: "Marques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Voitures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Immatriculation = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Annee = table.Column<int>(type: "INTEGER", nullable: false),
                    TarifJournalier = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NombrePlaces = table.Column<int>(type: "INTEGER", nullable: false),
                    Carburant = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ModeleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voitures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voitures_Modeles_ModeleId",
                        column: x => x.ModeleId,
                        principalTable: "Modeles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateDebut = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VoitureId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Voitures_VoitureId",
                        column: x => x.VoitureId,
                        principalTable: "Voitures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Email", "Nom", "Prenom", "Telephone" },
                values: new object[,]
                {
                    { 1, "lucas.martin@example.com", "Martin", "Lucas", "0601020304" },
                    { 2, "emma.bernard@example.com", "Bernard", "Emma", "0605060708" }
                });

            migrationBuilder.InsertData(
                table: "Marques",
                columns: new[] { "Id", "Nom", "PaysOrigine" },
                values: new object[,]
                {
                    { 1, "Renault", "France" },
                    { 2, "Peugeot", "France" },
                    { 3, "BMW", "Allemagne" }
                });

            migrationBuilder.InsertData(
                table: "Modeles",
                columns: new[] { "Id", "MarqueId", "Nom" },
                values: new object[,]
                {
                    { 1, 1, "Clio" },
                    { 2, 1, "Captur" },
                    { 3, 2, "208" },
                    { 4, 2, "308" },
                    { 5, 3, "Série 3" },
                    { 6, 3, "X1" }
                });

            migrationBuilder.InsertData(
                table: "Voitures",
                columns: new[] { "Id", "Annee", "Carburant", "Immatriculation", "ModeleId", "NombrePlaces", "TarifJournalier" },
                values: new object[,]
                {
                    { 1, 2021, "Essence", "AA-123-BB", 1, 5, 39.90m },
                    { 2, 2022, "Diesel", "CD-456-EF", 2, 5, 45.00m },
                    { 3, 2023, "Hybride", "GH-789-IJ", 3, 5, 52.50m },
                    { 4, 2020, "Essence", "KL-012-MN", 5, 5, 75.00m },
                    { 5, 2024, "Electrique", "OP-345-QR", 6, 5, 89.00m }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "ClientId", "DateDebut", "DateFin", "Montant", "VoitureId" },
                values: new object[] { 1, 1, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5), 159.60m, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marques_Nom",
                table: "Marques",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modeles_MarqueId",
                table: "Modeles",
                column: "MarqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ClientId",
                table: "Reservations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VoitureId",
                table: "Reservations",
                column: "VoitureId");

            migrationBuilder.CreateIndex(
                name: "IX_Voitures_Immatriculation",
                table: "Voitures",
                column: "Immatriculation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voitures_ModeleId",
                table: "Voitures",
                column: "ModeleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Voitures");

            migrationBuilder.DropTable(
                name: "Modeles");

            migrationBuilder.DropTable(
                name: "Marques");
        }
    }
}
