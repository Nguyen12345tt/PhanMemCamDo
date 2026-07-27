using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanMemCamDo.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "PaymentHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "PaymentHistories");
        }
    }
}
