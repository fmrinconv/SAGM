using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    InvoiceDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Folio = table.Column<int>(type: "int", nullable: true),
                    UUID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UM = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PU = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDetails", x => x.InvoiceDetailId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceDetails");
        }
    }
}
