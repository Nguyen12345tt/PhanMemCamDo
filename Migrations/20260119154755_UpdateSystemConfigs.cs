using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanMemCamDo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SystemConfigs",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "SystemConfigs",
                newName: "ConfigKey");

            migrationBuilder.AddColumn<string>(
                name: "ConfigValue",
                table: "SystemConfigs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigValue",
                table: "SystemConfigs");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "SystemConfigs",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "ConfigKey",
                table: "SystemConfigs",
                newName: "Key");
        }
    }
}
