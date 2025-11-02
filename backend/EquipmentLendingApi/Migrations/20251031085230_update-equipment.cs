using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentLendingApi.Migrations
{
    /// <inheritdoc />
    public partial class updateequipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Equipment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Equipment");
        }
    }
}
