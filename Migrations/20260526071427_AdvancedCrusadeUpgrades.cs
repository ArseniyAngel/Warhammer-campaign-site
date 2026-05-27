using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedCrusadeUpgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpgradeId",
                table: "Squads");

            migrationBuilder.AddColumn<int>(
                name: "PtsModifier",
                table: "CrusadeTraits",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SquadUpgrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SquadId = table.Column<int>(type: "INTEGER", nullable: false),
                    CrusadeTraitId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SquadUpgrades", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SquadUpgrades");

            migrationBuilder.DropColumn(
                name: "PtsModifier",
                table: "CrusadeTraits");

            migrationBuilder.AddColumn<int>(
                name: "UpgradeId",
                table: "Squads",
                type: "INTEGER",
                nullable: true);
        }
    }
}
