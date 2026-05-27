using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGMMarksAndUndergroundToSectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Sectors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GMMarks",
                table: "Sectors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderground",
                table: "Sectors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "GMMarks",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "IsUnderground",
                table: "Sectors");
        }
    }
}
