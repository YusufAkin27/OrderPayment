using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderPayment.Migrations
{
    /// <inheritdoc />
    public partial class dsada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cvc",
                table: "UserCards",
                newName: "CVV");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CVV",
                table: "UserCards",
                newName: "Cvc");
        }
    }
}
