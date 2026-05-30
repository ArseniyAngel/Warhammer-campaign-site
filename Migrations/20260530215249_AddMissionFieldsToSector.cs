using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionFieldsToSector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilesJson",
                table: "Sectors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MissionName",
                table: "Sectors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MissionStatus",
                table: "Sectors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilesJson",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "MissionName",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "MissionStatus",
                table: "Sectors");
        }
    }
}
