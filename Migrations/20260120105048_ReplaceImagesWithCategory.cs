using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhanMemCamDo.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceImagesWithCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AssetCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Xe Máy" },
                    { 2, "Ô Tô" },
                    { 3, "Điện Thoại" },
                    { 4, "Máy Tính / Laptop" },
                    { 5, "Trang Sức / Vàng" },
                    { 6, "Đồng Hồ" },
                    { 7, "Giấy Tờ Xe / Nhà Đất" },
                    { 8, "Khác" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AssetCategories",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
