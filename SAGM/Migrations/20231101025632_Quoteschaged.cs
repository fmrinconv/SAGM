using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class Quoteschaged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Sellers_SellerId",
                table: "Quotes");

            migrationBuilder.DropTable(
                name: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_SellerId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "FinalUser",
                table: "Quotes");

            migrationBuilder.RenameColumn(
                name: "SellerId",
                table: "Quotes",
                newName: "FinalUserId");

            migrationBuilder.AddColumn<string>(
                name: "Seller",
                table: "Quotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seller",
                table: "Quotes");

            migrationBuilder.RenameColumn(
                name: "FinalUserId",
                table: "Quotes",
                newName: "SellerId");

            migrationBuilder.AddColumn<int>(
                name: "FinalUser",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    SellerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.SellerId);
                    table.ForeignKey(
                        name: "FK_Sellers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_SellerId",
                table: "Quotes",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_UserId",
                table: "Sellers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Sellers_SellerId",
                table: "Quotes",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "SellerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
