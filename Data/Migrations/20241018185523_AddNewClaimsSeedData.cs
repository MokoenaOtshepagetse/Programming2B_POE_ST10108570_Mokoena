using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claimed.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewClaimsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "ClaimId",
                keyValue: 1,
                column: "DateSubmitted",
                value: new DateTime(2024, 10, 18, 20, 55, 21, 911, DateTimeKind.Local).AddTicks(6441));

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Users_UserId",
                table: "Claims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Users_UserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_UserId",
                table: "Claims");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "ClaimId",
                keyValue: 1,
                column: "DateSubmitted",
                value: new DateTime(2024, 10, 18, 13, 53, 18, 421, DateTimeKind.Local).AddTicks(6742));
        }
    }
}
