using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class Deliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderDeliveries",
                columns: table => new
                {
                    WorkOrderDeliveryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderDeliveryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    RemisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderDeliveries", x => x.WorkOrderDeliveryId);
                    table.ForeignKey(
                        name: "FK_WorkOrderDeliveries_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId");
                });

            migrationBuilder.CreateTable(
                name: "workOrderDeliveryDetails",
                columns: table => new
                {
                    WorkOrderDeliverDetailyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderDeliveryId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workOrderDeliveryDetails", x => x.WorkOrderDeliverDetailyId);
                    table.ForeignKey(
                        name: "FK_workOrderDeliveryDetails_WorkOrderDeliveries_WorkOrderDeliveryId",
                        column: x => x.WorkOrderDeliveryId,
                        principalTable: "WorkOrderDeliveries",
                        principalColumn: "WorkOrderDeliveryId");
                    table.ForeignKey(
                        name: "FK_workOrderDeliveryDetails_WorkOrderDetails_WorkOrderDetailId",
                        column: x => x.WorkOrderDetailId,
                        principalTable: "WorkOrderDetails",
                        principalColumn: "WorkOrderDetailId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDeliveries_WorkOrderId",
                table: "WorkOrderDeliveries",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_workOrderDeliveryDetails_WorkOrderDeliveryId",
                table: "workOrderDeliveryDetails",
                column: "WorkOrderDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_workOrderDeliveryDetails_WorkOrderDetailId",
                table: "workOrderDeliveryDetails",
                column: "WorkOrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workOrderDeliveryDetails");

            migrationBuilder.DropTable(
                name: "WorkOrderDeliveries");
        }
    }
}
