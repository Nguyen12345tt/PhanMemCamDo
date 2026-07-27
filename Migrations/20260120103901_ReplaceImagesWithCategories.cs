using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanMemCamDo.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceImagesWithCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetImages");

            migrationBuilder.AlterColumn<string>(
                name: "AssetName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "AssetCategoryId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetCategoryId",
                table: "Assets",
                column: "AssetCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AssetCategories_AssetCategoryId",
                table: "Assets",
                column: "AssetCategoryId",
                principalTable: "AssetCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AssetCategories_AssetCategoryId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "AssetCategories");

            migrationBuilder.DropIndex(
                name: "IX_Assets_AssetCategoryId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssetCategoryId",
                table: "Assets");

            migrationBuilder.AlterColumn<string>(
                name: "AssetName",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AssetImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetImages_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetImages_AssetId",
                table: "AssetImages",
                column: "AssetId");
        }
    }
}
