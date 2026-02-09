using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingIntelli.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithActionItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meetings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    meeting_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    attendees = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meetings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "action_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    meeting_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignee = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    task = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_action_items_meetings",
                        column: x => x.meeting_id,
                        principalTable: "meetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_action_items_assignee",
                table: "action_items",
                column: "assignee");

            migrationBuilder.CreateIndex(
                name: "IX_action_items_due_date",
                table: "action_items",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_action_items_meeting_id",
                table: "action_items",
                column: "meeting_id");

            migrationBuilder.CreateIndex(
                name: "IX_action_items_priority",
                table: "action_items",
                column: "priority");

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
                name: "action_items");

            migrationBuilder.DropTable(
                name: "meetings");
        }
    }
}
