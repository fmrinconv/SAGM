using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class linkOrderWithWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WorkOrderId",
                table: "Orders",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_WorkOrders_WorkOrderId",
                table: "Orders",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_WorkOrders_WorkOrderId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WorkOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "Orders");
        }
    }
}
