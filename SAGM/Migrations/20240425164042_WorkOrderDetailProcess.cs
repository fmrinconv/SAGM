using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrderDetailProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderDetailProcesses",
                columns: table => new
                {
                    WorkOrderDetailProcessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderDetailProcesses", x => x.WorkOrderDetailProcessId);
                    table.ForeignKey(
                        name: "FK_WorkOrderDetailProcesses_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId");
                    table.ForeignKey(
                        name: "FK_WorkOrderDetailProcesses_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId");
                    table.ForeignKey(
                        name: "FK_WorkOrderDetailProcesses_WorkOrderDetails_WorkOrderDetailId",
                        column: x => x.WorkOrderDetailId,
                        principalTable: "WorkOrderDetails",
                        principalColumn: "WorkOrderDetailId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDetailProcesses_MachineId",
                table: "WorkOrderDetailProcesses",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDetailProcesses_UnitId",
                table: "WorkOrderDetailProcesses",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDetailProcesses_WorkOrderDetailId",
                table: "WorkOrderDetailProcesses",
                column: "WorkOrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderDetailProcesses");
        }
    }
}
