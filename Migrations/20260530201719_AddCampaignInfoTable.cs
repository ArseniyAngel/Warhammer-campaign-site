using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignInfos",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInfos", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignInfos");
        }
    }
}
