using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingIntelli.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meetings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    meeting_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    attendees = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    action_items_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meetings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meetings_created_at",
                table: "meetings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_meeting_date",
                table: "meetings",
                column: "meeting_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meetings");
        }
    }
}
