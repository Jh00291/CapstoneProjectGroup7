using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TicketSystemWeb.Migrations
{
    /// <inheritdoc />
    public partial class TestSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "ClosedAt", "CreatedAt", "CreatedBy", "Description", "Status", "Title" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 2, 4, 19, 16, 20, 438, DateTimeKind.Utc).AddTicks(6919), "Admin", "This is a test ticket.", "Open", "First Ticket" },
                    { 2, null, new DateTime(2025, 2, 4, 19, 16, 20, 438, DateTimeKind.Utc).AddTicks(7156), "User123", "Another test ticket.", "In Progress", "Second Ticket" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: 2);
        }
    }
}
