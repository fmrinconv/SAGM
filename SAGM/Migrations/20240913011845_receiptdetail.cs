using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class receiptdetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetailReceipts");

            migrationBuilder.CreateTable(
                name: "OrderReceiptDetails",
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
                    table.PrimaryKey("PK_OrderReceiptDetails", x => x.OrderDetailReceiptId);
                    table.ForeignKey(
                        name: "FK_OrderReceiptDetails_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_OrderReceiptDetails_OrderReceipts_OrderReceiptId",
                        column: x => x.OrderReceiptId,
                        principalTable: "OrderReceipts",
                        principalColumn: "OrderReceiptId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderReceiptDetails_OrderDetailId",
                table: "OrderReceiptDetails",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReceiptDetails_OrderReceiptId",
                table: "OrderReceiptDetails",
                column: "OrderReceiptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderReceiptDetails");

            migrationBuilder.CreateTable(
                name: "OrderDetailReceipts",
                columns: table => new
                {
                    OrderDetailReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDetailId = table.Column<int>(type: "int", nullable: true),
                    OrderReceiptId = table.Column<int>(type: "int", nullable: true),
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
        }
    }
}
