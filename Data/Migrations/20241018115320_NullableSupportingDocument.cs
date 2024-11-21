using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Claimed.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableSupportingDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SupportingDocument",
                table: "Claims",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "ClaimId", "Course", "DateReviewed", "DateSubmitted", "Description", "HourlyRate", "HoursWorked", "Status", "SupportingDocument", "UserId" },
                values: new object[] { 1, "Programming 1A", null, new DateTime(2024, 10, 18, 13, 53, 18, 421, DateTimeKind.Local).AddTicks(6742), "Claim for completing all of Prog 1A before mid-term break", 50.00m, 10.5, "Pending", null, 1 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "IsAdmin", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { 1, false, "user1password", "user1" },
                    { 2, false, "user2password", "user2" },
                    { 3, true, "admin1password", "admin1" },
                    { 4, true, "admin2password", "admin2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "ClaimId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4);

            migrationBuilder.AlterColumn<string>(
                name: "SupportingDocument",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
