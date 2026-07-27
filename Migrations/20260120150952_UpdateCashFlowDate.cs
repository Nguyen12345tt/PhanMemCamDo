using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanMemCamDo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCashFlowDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "CashFlows",
                newName: "FlowType");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "CashFlows",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "CashFlows",
                newName: "UserName");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CashFlows",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CashFlows");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "CashFlows",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "FlowType",
                table: "CashFlows",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "CashFlows",
                newName: "TransactionDate");
        }
    }
}
