using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAGM.Migrations
{
    /// <inheritdoc />
    public partial class Archives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archive",
                columns: table => new
                {
                    ArchiveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArchiveName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archive", x => x.ArchiveId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archive");
        }
    }
}
