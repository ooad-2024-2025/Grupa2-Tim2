using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStripePaymentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RezervacijaId",
                table: "Placanje",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Placanje",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "Placanje",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Placanje_RezervacijaId",
                table: "Placanje",
                column: "RezervacijaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Placanje_Rezervacija_RezervacijaId",
                table: "Placanje",
                column: "RezervacijaId",
                principalTable: "Rezervacija",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Placanje_Rezervacija_RezervacijaId",
                table: "Placanje");

            migrationBuilder.DropIndex(
                name: "IX_Placanje_RezervacijaId",
                table: "Placanje");

            migrationBuilder.DropColumn(
                name: "RezervacijaId",
                table: "Placanje");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Placanje");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "Placanje");
        }
    }
}
