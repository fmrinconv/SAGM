using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class ProcessAndMachines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailComment_AspNetUsers_UserId",
                table: "WorkOrderDetailComment");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailComment_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderDetailComment",
                table: "WorkOrderDetailComment");

            migrationBuilder.RenameTable(
                name: "WorkOrderDetailComment",
                newName: "WorkOrderDetailComments");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetailComment_WorkOrderDetailId",
                table: "WorkOrderDetailComments",
                newName: "IX_WorkOrderDetailComments_WorkOrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetailComment_UserId",
                table: "WorkOrderDetailComments",
                newName: "IX_WorkOrderDetailComments_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderDetailComments",
                table: "WorkOrderDetailComments",
                column: "CommentId");

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    ProcessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.ProcessId);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ProcessesProcessId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.MachineId);
                    table.ForeignKey(
                        name: "FK_Machines_Processes_ProcessesProcessId",
                        column: x => x.ProcessesProcessId,
                        principalTable: "Processes",
                        principalColumn: "ProcessId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_ProcessesProcessId",
                table: "Machines",
                column: "ProcessesProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailComments_AspNetUsers_UserId",
                table: "WorkOrderDetailComments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailComments_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailComments",
                column: "WorkOrderDetailId",
                principalTable: "WorkOrderDetails",
                principalColumn: "WorkOrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailComments_AspNetUsers_UserId",
                table: "WorkOrderDetailComments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderDetailComments_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailComments");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderDetailComments",
                table: "WorkOrderDetailComments");

            migrationBuilder.RenameTable(
                name: "WorkOrderDetailComments",
                newName: "WorkOrderDetailComment");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetailComments_WorkOrderDetailId",
                table: "WorkOrderDetailComment",
                newName: "IX_WorkOrderDetailComment_WorkOrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderDetailComments_UserId",
                table: "WorkOrderDetailComment",
                newName: "IX_WorkOrderDetailComment_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderDetailComment",
                table: "WorkOrderDetailComment",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailComment_AspNetUsers_UserId",
                table: "WorkOrderDetailComment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderDetailComment_WorkOrderDetails_WorkOrderDetailId",
                table: "WorkOrderDetailComment",
                column: "WorkOrderDetailId",
                principalTable: "WorkOrderDetails",
                principalColumn: "WorkOrderDetailId");
        }
    }
}
