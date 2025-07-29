using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncArea.Migrations
{
    /// <inheritdoc />
    public partial class 修改密码机制 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Workspaces");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Workspaces",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Workspaces");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Workspaces",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
