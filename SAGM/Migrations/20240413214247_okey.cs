using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class okey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetail_Materials_MaterialId",
                table: "WorkOrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetail_Units_UnitId",
                table: "WorkOrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetail_WorkOrders_WorkOrderId",
                table: "WorkOrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailComment_WorkOrderDetail_WorkOrderDetailId",
                table: "WorkOrderDetailComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderDetail",
                table: "WorkOrderDetail");

            migrationBuilder.RenameTable(
                name: "WorkOrderDetail",
                newName: "WorkOrderDetails");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetail_WorkOrderId",
                table: "WorkOrderDetails",
                newName: "IX_WorkOrderDetails_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetail_UnitId",
                table: "WorkOrderDetails",
                newName: "IX_WorkOrderDetails_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetail_MaterialId",
                table: "WorkOrderDetails",
                newName: "IX_WorkOrderDetails_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderDetails",
                table: "WorkOrderDetails",
                column: "WorkOrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailComment_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailComment",
                column: "WorkOrderDetailId",
                principalTable: "WorkOrderDetails",
                principalColumn: "WorkOrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetails_Materials_MaterialId",
                table: "WorkOrderDetails",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetails_Units_UnitId",
                table: "WorkOrderDetails",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetails_WorkOrders_WorkOrderId",
                table: "WorkOrderDetails",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailComment_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailComment");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetails_Materials_MaterialId",
                table: "WorkOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetails_Units_UnitId",
                table: "WorkOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetails_WorkOrders_WorkOrderId",
                table: "WorkOrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderDetails",
                table: "WorkOrderDetails");

            migrationBuilder.RenameTable(
                name: "WorkOrderDetails",
                newName: "WorkOrderDetail");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetails_WorkOrderId",
                table: "WorkOrderDetail",
                newName: "IX_WorkOrderDetail_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetails_UnitId",
                table: "WorkOrderDetail",
                newName: "IX_WorkOrderDetail_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetails_MaterialId",
                table: "WorkOrderDetail",
                newName: "IX_WorkOrderDetail_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderDetail",
                table: "WorkOrderDetail",
                column: "WorkOrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetail_Materials_MaterialId",
                table: "WorkOrderDetail",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetail_Units_UnitId",
                table: "WorkOrderDetail",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetail_WorkOrders_WorkOrderId",
                table: "WorkOrderDetail",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailComment_WorkOrderDetail_WorkOrderDetailId",
                table: "WorkOrderDetailComment",
                column: "WorkOrderDetailId",
                principalTable: "WorkOrderDetail",
                principalColumn: "WorkOrderDetailId");
        }
    }
}
