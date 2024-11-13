using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPayment.Migrations
{
    /// <inheritdoc />
    public partial class sfaf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "products",
                newName: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "products",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
