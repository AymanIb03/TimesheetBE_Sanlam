using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Timesheet.Migrations
{
    /// <inheritdoc />
    public partial class assignements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Assignments",
                columns: new[] { "Id", "Description", "DueDate", "ProjectId", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, "Description 1", new DateTime(2024, 8, 9, 12, 15, 9, 529, DateTimeKind.Local).AddTicks(6269), 1, "Assignment 1", "c36ed951-bdb2-4e42-be47-5f887c0b79f5" },
                    { 2, "Description 2", new DateTime(2024, 8, 9, 12, 15, 9, 529, DateTimeKind.Local).AddTicks(6346), 2, "Assignment 2", "7c4b6ea5-3225-48dc-bcb1-b9bca36ae522" },
                    { 3, "Description 3", new DateTime(2024, 8, 9, 12, 15, 9, 529, DateTimeKind.Local).AddTicks(6349), 3, "Assignment 3", "c36ed951-bdb2-4e42-be47-5f887c0b79f5" },
                    { 4, "Description 4", new DateTime(2024, 8, 9, 12, 15, 9, 529, DateTimeKind.Local).AddTicks(6352), 4, "Assignment 4", "7c4b6ea5-3225-48dc-bcb1-b9bca36ae522" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
