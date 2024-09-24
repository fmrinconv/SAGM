using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class receipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderReceipts",
                columns: table => new
                {
                    OrderReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    ReceiptName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderReceipts", x => x.OrderReceiptId);
                    table.ForeignKey(
                        name: "FK_OrderReceipts_AspNetUsers_ReceivedById",
                        column: x => x.ReceivedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderReceipts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetailReceipts",
                columns: table => new
                {
                    OrderDetailReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderReceiptId = table.Column<int>(type: "int", nullable: true),
                    OrderDetailId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetailReceipts", x => x.OrderDetailReceiptId);
                    table.ForeignKey(
                        name: "FK_OrderDetailReceipts_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_OrderDetailReceipts_OrderReceipts_OrderReceiptId",
                        column: x => x.OrderReceiptId,
                        principalTable: "OrderReceipts",
                        principalColumn: "OrderReceiptId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetailReceipts_OrderDetailId",
                table: "OrderDetailReceipts",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetailReceipts_OrderReceiptId",
                table: "OrderDetailReceipts",
                column: "OrderReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReceipts_OrderId",
                table: "OrderReceipts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReceipts_ReceivedById",
                table: "OrderReceipts",
                column: "ReceivedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetailReceipts");

            migrationBuilder.DropTable(
                name: "OrderReceipts");
        }
    }
}
