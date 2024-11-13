using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPayment.Migrations
{
    /// <inheritdoc />
    public partial class fsida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "VerificationCodes");

            migrationBuilder.AddColumn<int>(
                name: "ExpiryInSeconds",
                table: "VerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryInSeconds",
                table: "VerificationCodes");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "VerificationCodes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
