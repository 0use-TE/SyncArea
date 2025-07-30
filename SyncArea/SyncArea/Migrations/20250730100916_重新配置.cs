using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncArea.Migrations
{
    /// <inheritdoc />
    public partial class 重新配置 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Photos");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Photos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Photos");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Photos",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
