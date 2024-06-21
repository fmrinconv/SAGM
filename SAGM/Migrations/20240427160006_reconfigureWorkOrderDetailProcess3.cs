using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class reconfigureWorkOrderDetailProcess3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailProcesses_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.AlterColumn<int>(
                name: "WorkOrderDetailId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailProcesses_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailProcesses",
                column: "WorkOrderDetailId",
                principalTable: "WorkOrderDetails",
                principalColumn: "WorkOrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailProcesses_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.AlterColumn<int>(
                name: "WorkOrderDetailId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailProcesses_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailProcesses",
                column: "WorkOrderDetailId",
                principalTable: "WorkOrderDetails",
                principalColumn: "WorkOrderDetailId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
