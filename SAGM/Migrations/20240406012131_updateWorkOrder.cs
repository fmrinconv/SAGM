using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class updateWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Invoiced",
                table: "WorkOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Machined",
                table: "WorkOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RawMaterial",
                table: "WorkOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Shipped",
                table: "WorkOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TT",
                table: "WorkOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Invoiced",
                table: "WorkOrderDetail");

            migrationBuilder.DropColumn(
                name: "Machined",
                table: "WorkOrderDetail");

            migrationBuilder.DropColumn(
                name: "RawMaterial",
                table: "WorkOrderDetail");

            migrationBuilder.DropColumn(
                name: "Shipped",
                table: "WorkOrderDetail");

            migrationBuilder.DropColumn(
                name: "TT",
                table: "WorkOrderDetail");
        }
    }
}
