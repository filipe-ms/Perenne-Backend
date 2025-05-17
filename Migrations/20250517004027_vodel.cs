using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace perenne.Migrations
{
    /// <inheritdoc />
    public partial class vodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Users_CreatedById",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Users_UpdatedById",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_CreatedById",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UpdatedById",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GroupMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "GroupMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_CreatedById",
                table: "GroupMembers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UpdatedById",
                table: "GroupMembers",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Users_CreatedById",
                table: "GroupMembers",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Users_UpdatedById",
                table: "GroupMembers",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
