using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncArea.Migrations
{
    /// <inheritdoc />
    public partial class 重新配置外键关系 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_WorkItems_WorkItemId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_AspNetUsers_UserId",
                table: "WorkItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkItemId",
                table: "Photos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_WorkItems_WorkItemId",
                table: "Photos",
                column: "WorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_AspNetUsers_UserId",
                table: "WorkItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_WorkItems_WorkItemId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_AspNetUsers_UserId",
                table: "WorkItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkItemId",
                table: "Photos",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_WorkItems_WorkItemId",
                table: "Photos",
                column: "WorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_AspNetUsers_UserId",
                table: "WorkItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
