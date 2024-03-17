using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuoteDetailComments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteDetailId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateComment = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDetailComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_QuoteDetailComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuoteDetailComments_QuoteDetails_QuoteDetailId",
                        column: x => x.QuoteDetailId,
                        principalTable: "QuoteDetails",
                        principalColumn: "QuoteDetailId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDetailComments_QuoteDetailId",
                table: "QuoteDetailComments",
                column: "QuoteDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDetailComments_UserId",
                table: "QuoteDetailComments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteDetailComments");
        }
    }
}
