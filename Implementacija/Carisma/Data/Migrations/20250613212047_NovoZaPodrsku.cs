using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class NovoZaPodrsku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Podrska_Osoba_KorisnikId",
                table: "Podrska");

            migrationBuilder.DropIndex(
                name: "IX_Podrska_KorisnikId",
                table: "Podrska");

            migrationBuilder.AlterColumn<string>(
                name: "Pitanje",
                table: "Podrska",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Podrska",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Hitnost",
                table: "Podrska",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "KomentarOcjene",
                table: "Podrska",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OcjenaKorisnika",
                table: "Podrska",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OsobaId",
                table: "Podrska",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PodrskaKorisnikId",
                table: "Podrska",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ime",
                table: "Osoba",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prezime",
                table: "Osoba",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Podrska_OsobaId",
                table: "Podrska",
                column: "OsobaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Podrska_Osoba_OsobaId",
                table: "Podrska",
                column: "OsobaId",
                principalTable: "Osoba",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Podrska_Osoba_OsobaId",
                table: "Podrska");

            migrationBuilder.DropIndex(
                name: "IX_Podrska_OsobaId",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "Hitnost",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "KomentarOcjene",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "OcjenaKorisnika",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "OsobaId",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "PodrskaKorisnikId",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Osoba");

            migrationBuilder.DropColumn(
                name: "Ime",
                table: "Osoba");

            migrationBuilder.DropColumn(
                name: "Prezime",
                table: "Osoba");

            migrationBuilder.AlterColumn<string>(
                name: "Pitanje",
                table: "Podrska",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<int>(
                name: "KorisnikId",
                table: "Podrska",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Podrska_KorisnikId",
                table: "Podrska",
                column: "KorisnikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Podrska_Osoba_KorisnikId",
                table: "Podrska",
                column: "KorisnikId",
                principalTable: "Osoba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
