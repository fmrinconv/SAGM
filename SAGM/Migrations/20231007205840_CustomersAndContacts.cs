using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class CustomersAndContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Cities_CityId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_States_Customers_CustomerId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_States_CustomerId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_CityId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "States");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Contacts");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Contacts");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "States",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Contacts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Contacts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_CustomerId",
                table: "States",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CityId",
                table: "Contacts",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Cities_CityId",
                table: "Contacts",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_States_Customers_CustomerId",
                table: "States",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");
        }
    }
}
