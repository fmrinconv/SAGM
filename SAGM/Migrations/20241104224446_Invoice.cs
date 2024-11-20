using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class Invoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Serie = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Folio = table.Column<int>(type: "int", nullable: true),
                    UUID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FechadeEmisión = table.Column<DateTime>(name: "Fecha de Emisión", type: "datetime2", nullable: true),
                    RFCEmisor = table.Column<string>(name: "RFC Emisor", type: "nvarchar(36)", maxLength: 36, nullable: true),
                    NombredelEmisor = table.Column<string>(name: "Nombre del Emisor", type: "nvarchar(36)", maxLength: 36, nullable: true),
                    RFCReceptor = table.Column<string>(name: "RFC Receptor", type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IVATrasladado = table.Column<decimal>(name: "IVA Trasladado", type: "decimal(18,2)", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    EstadoFiscal = table.Column<string>(name: "Estado Fiscal", type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Pagado = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TipodeComprobante = table.Column<string>(name: "Tipo de Comprobante", type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TipodeCFDI = table.Column<string>(name: "Tipo de CFDI", type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Fechadepago = table.Column<DateTime>(name: "Fecha de pago", type: "datetime2", nullable: true),
                    Comentariodepago = table.Column<string>(name: "Comentario de pago", type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Metododepago = table.Column<string>(name: "Metodo de pago", type: "nvarchar(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
