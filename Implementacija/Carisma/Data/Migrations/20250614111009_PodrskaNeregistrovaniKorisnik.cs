using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class PodrskaNeregistrovaniKorisnik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Podrska",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImePrezime",
                table: "Podrska",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "ImePrezime",
                table: "Podrska");
        }
    }
}
