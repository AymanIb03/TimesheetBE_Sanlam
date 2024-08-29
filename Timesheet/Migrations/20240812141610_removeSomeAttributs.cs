using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timesheet.Migrations
{
    public partial class removeSomeAttributs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectCode",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Assignments");

            // Si vous souhaitez mettre à jour les valeurs de DueDate, assurez-vous que la colonne existe dans la base de données.
            // Supprimez les sections suivantes si cette colonne n'existe pas.
            // Sinon, vous pouvez laisser le code suivant si vous êtes sûr que DueDate existe.

            /*
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
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Si vous voulez remettre les valeurs de DueDate, assurez-vous que la colonne existe dans la base de données.
            // Supprimez les sections suivantes si cette colonne n'existe pas.
            // Sinon, vous pouvez laisser le code suivant si vous êtes sûr que DueDate existe.

            /*
            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "DueDate", "Title" },
                values: new object[] { "Description 1", new DateTime(2024, 8, 12, 15, 6, 37, 385, DateTimeKind.Local).AddTicks(1944), "Assignment 1" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "DueDate", "Title" },
                values: new object[] { "Description 2", new DateTime(2024, 8, 12, 15, 6, 37, 385, DateTimeKind.Local).AddTicks(1999), "Assignment 2" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DueDate", "Title" },
                values: new object[] { "Description 3", new DateTime(2024, 8, 12, 15, 6, 37, 385, DateTimeKind.Local).AddTicks(2001), "Assignment 3" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "DueDate", "Title" },
                values: new object[] { "Description 4", new DateTime(2024, 8, 12, 15, 6, 37, 385, DateTimeKind.Local).AddTicks(2003), "Assignment 4" });
            */

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProjectCode",
                value: "P001");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProjectCode",
                value: "P002");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProjectCode",
                value: "P003");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 4,
                column: "ProjectCode",
                value: "P004");
        }
    }
}
