using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace perenne.Migrations
{
    /// <inheritdoc />
    public partial class ModelFixing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "GroupMembers");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Users",
                newName: "SystemRole");

            migrationBuilder.RenameColumn(
                name: "IsMutedInGroupChat",
                table: "GroupMembers",
                newName: "IsMuted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SystemRole",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "IsMuted",
                table: "GroupMembers",
                newName: "IsMutedInGroupChat");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "GroupMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GroupMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "GroupMembers",
                type: "uuid",
                nullable: true);
        }
    }
}
