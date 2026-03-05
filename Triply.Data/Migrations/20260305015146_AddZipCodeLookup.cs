using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Triply.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddZipCodeLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZipCodeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StateAbbr = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    County = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZipCodeLookups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZipCodeLookups_ZipCode",
                table: "ZipCodeLookups",
                column: "ZipCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZipCodeLookups");
        }
    }
}
