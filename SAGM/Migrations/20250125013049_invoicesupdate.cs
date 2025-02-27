using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class invoicesupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha de carga",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo de carga",
                table: "Invoices",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha de carga",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Tipo de carga",
                table: "Invoices");
        }
    }
}
