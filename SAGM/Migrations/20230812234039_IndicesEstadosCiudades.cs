using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class IndicesEstadosCiudades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_States_StateName_CountryId",
                table: "States",
                columns: new[] { "StateName", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName_StateId",
                table: "Cities",
                columns: new[] { "CityName", "StateId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_States_StateName_CountryId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_Cities_CityName_StateId",
                table: "Cities");
        }
    }
}
