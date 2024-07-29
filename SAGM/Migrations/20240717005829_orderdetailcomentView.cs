using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class orderdetailcomentView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderDetailCommentCommentId",
                table: "OrderDetailComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetailComments_OrderDetailCommentCommentId",
                table: "OrderDetailComments",
                column: "OrderDetailCommentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetailComments_OrderDetailComments_OrderDetailCommentCommentId",
                table: "OrderDetailComments",
                column: "OrderDetailCommentCommentId",
                principalTable: "OrderDetailComments",
                principalColumn: "CommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetailComments_OrderDetailComments_OrderDetailCommentCommentId",
                table: "OrderDetailComments");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetailComments_OrderDetailCommentCommentId",
                table: "OrderDetailComments");

            migrationBuilder.DropColumn(
                name: "OrderDetailCommentCommentId",
                table: "OrderDetailComments");
        }
    }
}
