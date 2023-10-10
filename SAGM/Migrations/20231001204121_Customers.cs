using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class Customers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_States_StateId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialTypes_MaterialTypeId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialTypes_Categories_CategoryId",
                table: "MaterialTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_States_Countries_CountryId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_States_StateName_CountryId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_MaterialTypes_MaterialTypeName_CategoryId",
                table: "MaterialTypes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_MaterialName_MaterialTypeId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Cities_CityName_StateId",
                table: "Cities");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "States",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "MaterialTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaterialTypeId",
                table: "Materials",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "Cities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CityId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerNickName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreditDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_States_StateName_CountryId",
                table: "States",
                columns: new[] { "StateName", "CountryId" },
                unique: true,
                filter: "[CountryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTypes_MaterialTypeName_CategoryId",
                table: "MaterialTypes",
                columns: new[] { "MaterialTypeName", "CategoryId" },
                unique: true,
                filter: "[CategoryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_MaterialName_MaterialTypeId",
                table: "Materials",
                columns: new[] { "MaterialName", "MaterialTypeId" },
                unique: true,
                filter: "[MaterialTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName_StateId",
                table: "Cities",
                columns: new[] { "CityName", "StateId" },
                unique: true,
                filter: "[StateId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CityId",
                table: "Customers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerName",
                table: "Customers",
                column: "CustomerName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_States_StateId",
                table: "Cities",
                column: "StateId",
                principalTable: "States",
                principalColumn: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialTypes_MaterialTypeId",
                table: "Materials",
                column: "MaterialTypeId",
                principalTable: "MaterialTypes",
                principalColumn: "MaterialTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialTypes_Categories_CategoryId",
                table: "MaterialTypes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_States_Countries_CountryId",
                table: "States",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_States_StateId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialTypes_MaterialTypeId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialTypes_Categories_CategoryId",
                table: "MaterialTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_States_Countries_CountryId",
                table: "States");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_States_StateName_CountryId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_MaterialTypes_MaterialTypeName_CategoryId",
                table: "MaterialTypes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_MaterialName_MaterialTypeId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Cities_CityName_StateId",
                table: "Cities");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "States",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "MaterialTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaterialTypeId",
                table: "Materials",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "Cities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CityId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_StateName_CountryId",
                table: "States",
                columns: new[] { "StateName", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTypes_MaterialTypeName_CategoryId",
                table: "MaterialTypes",
                columns: new[] { "MaterialTypeName", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_MaterialName_MaterialTypeId",
                table: "Materials",
                columns: new[] { "MaterialName", "MaterialTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName_StateId",
                table: "Cities",
                columns: new[] { "CityName", "StateId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_States_StateId",
                table: "Cities",
                column: "StateId",
                principalTable: "States",
                principalColumn: "StateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialTypes_MaterialTypeId",
                table: "Materials",
                column: "MaterialTypeId",
                principalTable: "MaterialTypes",
                principalColumn: "MaterialTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialTypes_Categories_CategoryId",
                table: "MaterialTypes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_States_Countries_CountryId",
                table: "States",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
