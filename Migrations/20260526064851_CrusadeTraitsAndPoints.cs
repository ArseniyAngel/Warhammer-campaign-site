using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class CrusadeTraitsAndPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BattleScars",
                table: "Squads");

            migrationBuilder.RenameColumn(
                name: "Upgrades",
                table: "Squads",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ExperiencePoints",
                table: "Squads",
                newName: "PointsCost");

            migrationBuilder.AddColumn<int>(
                name: "ScarId",
                table: "Squads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpgradeId",
                table: "Squads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CrusadeTraits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    UnitTypeRestriction = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrusadeTraits", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrusadeTraits");

            migrationBuilder.DropColumn(
                name: "ScarId",
                table: "Squads");

            migrationBuilder.DropColumn(
                name: "UpgradeId",
                table: "Squads");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Squads",
                newName: "Upgrades");

            migrationBuilder.RenameColumn(
                name: "PointsCost",
                table: "Squads",
                newName: "ExperiencePoints");

            migrationBuilder.AddColumn<string>(
                name: "BattleScars",
                table: "Squads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
