using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class workordercomment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderComment_AspNetUsers_UserId",
                table: "WorkOrderComment");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderComment_WorkOrders_WorkOrderId",
                table: "WorkOrderComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderComment",
                table: "WorkOrderComment");

            migrationBuilder.RenameTable(
                name: "WorkOrderComment",
                newName: "WorkOrderComments");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderComment_WorkOrderId",
                table: "WorkOrderComments",
                newName: "IX_WorkOrderComments_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderComment_UserId",
                table: "WorkOrderComments",
                newName: "IX_WorkOrderComments_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderComments",
                table: "WorkOrderComments",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderComments_AspNetUsers_UserId",
                table: "WorkOrderComments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderComments_WorkOrders_WorkOrderId",
                table: "WorkOrderComments",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderComments_AspNetUsers_UserId",
                table: "WorkOrderComments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderComments_WorkOrders_WorkOrderId",
                table: "WorkOrderComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderComments",
                table: "WorkOrderComments");

            migrationBuilder.RenameTable(
                name: "WorkOrderComments",
                newName: "WorkOrderComment");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderComments_WorkOrderId",
                table: "WorkOrderComment",
                newName: "IX_WorkOrderComment_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderComments_UserId",
                table: "WorkOrderComment",
                newName: "IX_WorkOrderComment_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderComment",
                table: "WorkOrderComment",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderComment_AspNetUsers_UserId",
                table: "WorkOrderComment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderComment_WorkOrders_WorkOrderId",
                table: "WorkOrderComment",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "WorkOrderId");
        }
    }
}
