using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace perenne.Migrations
{
    /// <inheritdoc />
    public partial class privateChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "ChatChannels",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "ChatChannels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "User1Id",
                table: "ChatChannels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "User2Id",
                table: "ChatChannels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_GroupId_IsPrivate",
                table: "ChatChannels",
                columns: new[] { "GroupId", "IsPrivate" },
                unique: true,
                filter: "\"IsPrivate\" = false AND \"GroupId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_User1Id_User2Id_IsPrivate",
                table: "ChatChannels",
                columns: new[] { "User1Id", "User2Id", "IsPrivate" },
                unique: true,
                filter: "\"IsPrivate\" = true AND \"User1Id\" IS NOT NULL AND \"User2Id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannels_User2Id",
                table: "ChatChannels",
                column: "User2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatChannels_Users_User1Id",
                table: "ChatChannels",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatChannels_Users_User2Id",
                table: "ChatChannels",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatChannels_Users_User1Id",
                table: "ChatChannels");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatChannels_Users_User2Id",
                table: "ChatChannels");

            migrationBuilder.DropIndex(
                name: "IX_ChatChannels_GroupId_IsPrivate",
                table: "ChatChannels");

            migrationBuilder.DropIndex(
                name: "IX_ChatChannels_User1Id_User2Id_IsPrivate",
                table: "ChatChannels");

            migrationBuilder.DropIndex(
                name: "IX_ChatChannels_User2Id",
                table: "ChatChannels");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "ChatChannels");

            migrationBuilder.DropColumn(
                name: "User1Id",
                table: "ChatChannels");

            migrationBuilder.DropColumn(
                name: "User2Id",
                table: "ChatChannels");

            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "ChatChannels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
