using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class reconfigureWorkOrderDetailProcess2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDetailProcesses_MachineId",
                table: "WorkOrderDetailProcesses",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDetailProcesses_UnitId",
                table: "WorkOrderDetailProcesses",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailProcesses_Machines_MachineId",
                table: "WorkOrderDetailProcesses",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailProcesses_Units_UnitId",
                table: "WorkOrderDetailProcesses",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailProcesses_Machines_MachineId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailProcesses_Units_UnitId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderDetailProcesses_MachineId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderDetailProcesses_UnitId",
                table: "WorkOrderDetailProcesses");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessId",
                table: "WorkOrderDetailProcesses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
