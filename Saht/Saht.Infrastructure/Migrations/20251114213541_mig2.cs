using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saht.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    BlockerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => new { x.BlockerId, x.BlockedId });
                });

            migrationBuilder.CreateTable(
                name: "Mutes",
                columns: table => new
                {
                    MuterId = table.Column<Guid>(type: "uuid", nullable: false),
                    MutedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mutes", x => new { x.MuterId, x.MutedId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockedId",
                table: "Blocks",
                column: "BlockedId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockerId",
                table: "Blocks",
                column: "BlockerId");

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_MutedId",
                table: "Mutes",
                column: "MutedId");

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_MuterId",
                table: "Mutes",
                column: "MuterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Mutes");
        }
    }
}
