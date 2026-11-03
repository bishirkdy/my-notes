using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNotes.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReadingProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Percent = table.Column<int>(type: "integer", nullable: false),
                    LastReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingProgress_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReadingProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReadingProgress_NoteId",
                table: "ReadingProgress",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingProgress_UserId_LastReadAtUtc",
                table: "ReadingProgress",
                columns: new[] { "UserId", "LastReadAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReadingProgress_UserId_NoteId",
                table: "ReadingProgress",
                columns: new[] { "UserId", "NoteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReadingProgress");
        }
    }
}
