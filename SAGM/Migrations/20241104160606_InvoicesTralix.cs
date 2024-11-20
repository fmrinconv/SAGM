using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class InvoicesTralix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invoicesTralixes",
                columns: table => new
                {
                    Serie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechadeEmision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFCEmisor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombredelEmisor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFCReceptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombredelReceptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subtotal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IVATrasladado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoFiscal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pagado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipodeComprobante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipodeCFDI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechadePago = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComentariodePago = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetododePago = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoicesTralixes");
        }
    }
}
