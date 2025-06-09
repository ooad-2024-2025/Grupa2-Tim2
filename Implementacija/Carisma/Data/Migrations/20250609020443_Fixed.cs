using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Placanje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iznos = table.Column<double>(type: "float", nullable: false),
                    datumPlacanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    statusPlacanja = table.Column<int>(type: "int", nullable: false),
                    brojKartice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    korisnikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placanje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placanje_Osoba_korisnikId",
                        column: x => x.korisnikId,
                        principalTable: "Osoba",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rezervacija",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    datumPocetka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    datumZavrsetka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    datumRezervacije = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    cijena = table.Column<double>(type: "float", nullable: false),
                    korisnikId = table.Column<int>(type: "int", nullable: true),
                    voziloId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervacija", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rezervacija_Osoba_korisnikId",
                        column: x => x.korisnikId,
                        principalTable: "Osoba",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rezervacija_Vozilo_voziloId",
                        column: x => x.voziloId,
                        principalTable: "Vozilo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Placanje_korisnikId",
                table: "Placanje",
                column: "korisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacija_korisnikId",
                table: "Rezervacija",
                column: "korisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacija_voziloId",
                table: "Rezervacija",
                column: "voziloId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Placanje");

            migrationBuilder.DropTable(
                name: "Rezervacija");
        }
    }
}
