using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class ProcessAndMachine2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Processes_ProcessesProcessId",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "ProcessesProcessId",
                table: "Machines",
                newName: "ProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_ProcessesProcessId",
                table: "Machines",
                newName: "IX_Machines_ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ProcessName",
                table: "Processes",
                column: "ProcessName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineName_ProcessId",
                table: "Machines",
                columns: new[] { "MachineName", "ProcessId" },
                unique: true,
                filter: "[ProcessId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Processes_ProcessId",
                table: "Machines",
                column: "ProcessId",
                principalTable: "Processes",
                principalColumn: "ProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Processes_ProcessId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Processes_ProcessName",
                table: "Processes");

            migrationBuilder.DropIndex(
                name: "IX_Machines_MachineName_ProcessId",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "ProcessId",
                table: "Machines",
                newName: "ProcessesProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_ProcessId",
                table: "Machines",
                newName: "IX_Machines_ProcessesProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Processes_ProcessesProcessId",
                table: "Machines",
                column: "ProcessesProcessId",
                principalTable: "Processes",
                principalColumn: "ProcessId");
        }
    }
}
