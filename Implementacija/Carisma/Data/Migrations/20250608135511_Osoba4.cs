using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class Osoba4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdKorisnika",
                table: "Osoba");

            migrationBuilder.DropColumn(
                name: "Kontakt",
                table: "Osoba");

            migrationBuilder.DropColumn(
                name: "Kontofile",
                table: "Osoba");

            migrationBuilder.RenameColumn(
                name: "Uloga",
                table: "Osoba",
                newName: "uloga");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Osoba",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Telefon",
                table: "Osoba",
                newName: "lozinka");

            migrationBuilder.AlterColumn<int>(
                name: "uloga",
                table: "Osoba",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "blokiran",
                table: "Osoba",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "broj_telefona",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "korisnicko_ime",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blokiran",
                table: "Osoba");

            migrationBuilder.DropColumn(
                name: "broj_telefona",
                table: "Osoba");

            migrationBuilder.DropColumn(
                name: "korisnicko_ime",
                table: "Osoba");

            migrationBuilder.RenameColumn(
                name: "uloga",
                table: "Osoba",
                newName: "Uloga");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Osoba",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "lozinka",
                table: "Osoba",
                newName: "Telefon");

            migrationBuilder.AlterColumn<int>(
                name: "Uloga",
                table: "Osoba",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdKorisnika",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kontakt",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kontofile",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
