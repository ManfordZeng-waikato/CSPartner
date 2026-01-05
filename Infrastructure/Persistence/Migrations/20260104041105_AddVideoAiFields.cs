using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiDescription",
                table: "Videos",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiHighlightType",
                table: "Videos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AiLastError",
                table: "Videos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiStatus",
                table: "Videos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AiTagsJson",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiUpdatedAtUtc",
                table: "Videos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Videos_AiStatus",
                table: "Videos",
                column: "AiStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Videos_AiStatus",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AiDescription",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AiHighlightType",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AiLastError",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AiStatus",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AiTagsJson",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AiUpdatedAtUtc",
                table: "Videos");
        }
    }
}
