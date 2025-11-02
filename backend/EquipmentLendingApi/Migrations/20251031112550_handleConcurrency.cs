using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentLendingApi.Migrations
{
    /// <inheritdoc />
    public partial class handleConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Equipment",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Equipment");
        }
    }
}
