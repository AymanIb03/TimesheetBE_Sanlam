using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Timesheet.Migrations
{
    /// <inheritdoc />
    public partial class nullIsValidated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AlterColumn<bool>(
                name: "IsValidated",
                table: "Timesheets",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1,
                column: "DueDate",
                value: new DateTime(2024, 8, 15, 10, 31, 48, 798, DateTimeKind.Local).AddTicks(9481));

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 2,
                column: "DueDate",
                value: new DateTime(2024, 8, 15, 10, 31, 48, 798, DateTimeKind.Local).AddTicks(9538));

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 3,
                column: "DueDate",
                value: new DateTime(2024, 8, 15, 10, 31, 48, 798, DateTimeKind.Local).AddTicks(9540));

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 4,
                column: "DueDate",
                value: new DateTime(2024, 8, 15, 10, 31, 48, 798, DateTimeKind.Local).AddTicks(9542));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsValidated",
                table: "Timesheets",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1,
                column: "DueDate",
                value: new DateTime(2024, 8, 12, 15, 16, 10, 327, DateTimeKind.Local).AddTicks(2907));

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 2,
                column: "DueDate",
                value: new DateTime(2024, 8, 12, 15, 16, 10, 327, DateTimeKind.Local).AddTicks(2964));

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 3,
                column: "DueDate",
                value: new DateTime(2024, 8, 12, 15, 16, 10, 327, DateTimeKind.Local).AddTicks(2966));

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 4,
                column: "DueDate",
                value: new DateTime(2024, 8, 12, 15, 16, 10, 327, DateTimeKind.Local).AddTicks(2968));

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "ProjectName" },
                values: new object[,]
                {
                    { 1, "Project 1" },
                    { 2, "Project 2" },
                    { 3, "Project 3" },
                    { 4, "Project 4" }
                });
        }
    }
}
