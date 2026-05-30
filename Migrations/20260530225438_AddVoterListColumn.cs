using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class AddVoterListColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoterListJson",
                table: "Sectors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoterListJson",
                table: "Sectors");
        }
    }
}
